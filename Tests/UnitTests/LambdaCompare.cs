﻿using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnitTest;

public static class LambdaCompare
{
	public static bool Eq<TSource, TValue>(
		Expression<Func<TSource, TValue>> x,
		Expression<Func<TSource, TValue>> y)
	{
		return ExpressionsEqual(x, y, null, null);
	}

	public static bool Eq<TSource1, TSource2, TValue>(
		Expression<Func<TSource1, TSource2, TValue>> x,
		Expression<Func<TSource1, TSource2, TValue>> y)
	{
		return ExpressionsEqual(x, y, null, null);
	}

	public static Expression<Func<Expression<Func<TSource, TValue>>, bool>> Eq<TSource, TValue>(Expression<Func<TSource, TValue>> y)
	{
		return x => ExpressionsEqual(x, y, null, null);
	}

	static bool ExpressionsEqual(Expression x, Expression y, LambdaExpression rootX, LambdaExpression rootY)
	{
		if (ReferenceEquals(x, y))
		{
			return true;
		}

		if (x == null || y == null)
		{
			return false;
		}

		ConstantValue valueX = TryCalculateConstant(x);
		ConstantValue valueY = TryCalculateConstant(y);

		if (valueX.IsDefined && valueY.IsDefined)
		{
			return ValuesEqual(valueX.Value, valueY.Value);
		}

		if (x.NodeType != y.NodeType
			|| x.Type != y.Type)
		{
			if (IsAnonymousType(x.Type) && IsAnonymousType(y.Type))
			{
				throw new NotImplementedException("Comparison of Anonymous Types is not supported");
			}
			return false;
		}

		if (x is LambdaExpression lx)
		{
			LambdaExpression ly = (LambdaExpression)y;
			System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> paramsX = lx.Parameters;
			System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> paramsY = ly.Parameters;
			return CollectionsEqual(paramsX, paramsY, lx, ly) && ExpressionsEqual(lx.Body, ly.Body, lx, ly);
		}
		if (x is MemberExpression mex)
		{
			MemberExpression mey = (MemberExpression)y;
			return Equals(mex.Member, mey.Member) && ExpressionsEqual(mex.Expression, mey.Expression, rootX, rootY);
		}
		if (x is BinaryExpression bx)
		{
			BinaryExpression by = (BinaryExpression)y;
			return bx.Method == @by.Method && ExpressionsEqual(bx.Left, @by.Left, rootX, rootY) &&
				   ExpressionsEqual(bx.Right, @by.Right, rootX, rootY);
		}
		if (x is UnaryExpression ux)
		{
			UnaryExpression uy = (UnaryExpression)y;
			return ux.Method == uy.Method && ExpressionsEqual(ux.Operand, uy.Operand, rootX, rootY);
		}
		if (x is ParameterExpression px)
		{
			ParameterExpression py = (ParameterExpression)y;
			return rootX.Parameters.IndexOf(px) == rootY.Parameters.IndexOf(py);
		}
		if (x is MethodCallExpression methodCallExpression)
		{
			MethodCallExpression cx = methodCallExpression;
			MethodCallExpression cy = (MethodCallExpression)y;
			return cx.Method == cy.Method
				   && ExpressionsEqual(cx.Object, cy.Object, rootX, rootY)
				   && CollectionsEqual(cx.Arguments, cy.Arguments, rootX, rootY);
		}
		if (x is MemberInitExpression mix)
		{
			MemberInitExpression miy = (MemberInitExpression)y;
			return ExpressionsEqual(mix.NewExpression, miy.NewExpression, rootX, rootY)
				   && MemberInitsEqual(mix.Bindings, miy.Bindings, rootX, rootY);
		}
		if (x is NewArrayExpression newArrayExpression)
		{
			NewArrayExpression nx = newArrayExpression;
			NewArrayExpression ny = (NewArrayExpression)y;
			return CollectionsEqual(nx.Expressions, ny.Expressions, rootX, rootY);
		}
		if (x is NewExpression newExpression)
		{
			NewExpression nx = newExpression;
			NewExpression ny = (NewExpression)y;
			return
				Equals(nx.Constructor, ny.Constructor)
				&& CollectionsEqual(nx.Arguments, ny.Arguments, rootX, rootY)
				&& ((nx.Members == null && ny.Members == null)
					|| (nx.Members != null && ny.Members != null && CollectionsEqual(nx.Members, ny.Members)));
		}
		if (x is ConditionalExpression conditionalExpression)
		{
			ConditionalExpression cx = conditionalExpression;
			ConditionalExpression cy = (ConditionalExpression)y;
			return
				ExpressionsEqual(cx.Test, cy.Test, rootX, rootY)
				&& ExpressionsEqual(cx.IfFalse, cy.IfFalse, rootX, rootY)
				&& ExpressionsEqual(cx.IfTrue, cy.IfTrue, rootX, rootY);
		}

		throw new NotImplementedException(x.ToString());
	}

	static Boolean IsAnonymousType(Type type)
	{
		Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
		Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
		Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

		return isAnonymousType;
	}

	static bool MemberInitsEqual(ICollection<MemberBinding> bx, ICollection<MemberBinding> by, LambdaExpression rootX, LambdaExpression rootY)
	{
		if (bx.Count != by.Count)
		{
			return false;
		}

		if (bx.Concat(by).Any(b => b.BindingType != MemberBindingType.Assignment))
		{
			throw new NotImplementedException("Only MemberBindingType.Assignment is supported");
		}

		return
			bx.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i })
			.Join(
				  by.Cast<MemberAssignment>().OrderBy(b => b.Member.Name).Select((b, i) => new { Expr = b.Expression, b.Member, Index = i }),
				  o => o.Index, o => o.Index, (xe, ye) => new { XExpr = xe.Expr, XMember = xe.Member, YExpr = ye.Expr, YMember = ye.Member })
				  .All(o => Equals(o.XMember, o.YMember) && ExpressionsEqual(o.XExpr, o.YExpr, rootX, rootY));
	}

	static bool ValuesEqual(object x, object y)
	{
		return ReferenceEquals(x, y)
			|| (x is ICollection && y is ICollection ? CollectionsEqual((ICollection)x, (ICollection)y) : Equals(x, y));
	}

	static ConstantValue TryCalculateConstant(Expression e)
	{
		if (e is ConstantExpression constantExpression)
		{
			return new ConstantValue(true, (constantExpression).Value);
		}

		if (e is MemberExpression me)
		{
			ConstantValue parentValue = TryCalculateConstant(me.Expression);
			if (parentValue.IsDefined)
			{
				object result =
					me.Member is FieldInfo
						? ((FieldInfo)me.Member).GetValue(parentValue.Value)
						: ((PropertyInfo)me.Member).GetValue(parentValue.Value);
				return new ConstantValue(true, result);
			}
		}
		if (e is NewArrayExpression ae)
		{
			IEnumerable<ConstantValue> result = ae.Expressions.Select(TryCalculateConstant);
			if (result.All(i => i.IsDefined))
			{
				return new ConstantValue(true, result.Select(i => i.Value).ToArray());
			}
		}
		if (e is ConditionalExpression ce)
		{
			ConstantValue evaluatedTest = TryCalculateConstant(ce.Test);
			if (evaluatedTest.IsDefined)
			{
				return TryCalculateConstant(Equals(evaluatedTest.Value, true) ? ce.IfTrue : ce.IfFalse);
			}
		}

		return default;
	}

	static bool CollectionsEqual(IEnumerable<Expression> x, IEnumerable<Expression> y, LambdaExpression rootX, LambdaExpression rootY)
	{
		return x.Count() == y.Count()
			   && x.Select((e, i) => new { Expr = e, Index = i })
				   .Join(y.Select((e, i) => new { Expr = e, Index = i }),
						 o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
				   .All(o => ExpressionsEqual(o.X, o.Y, rootX, rootY));
	}

	static bool CollectionsEqual(ICollection x, ICollection y)
	{
		return x.Count == y.Count
			   && x.Cast<object>().Select((e, i) => new { Expr = e, Index = i })
				   .Join(y.Cast<object>().Select((e, i) => new { Expr = e, Index = i }),
						 o => o.Index, o => o.Index, (xe, ye) => new { X = xe.Expr, Y = ye.Expr })
				   .All(o => Equals(o.X, o.Y));
	}

	struct ConstantValue
	{
		public ConstantValue(bool isDefined, object value)
			: this()
		{
			IsDefined = isDefined;
			Value = value;
		}

		public bool IsDefined { get; }

		public object Value { get; }
	}
}