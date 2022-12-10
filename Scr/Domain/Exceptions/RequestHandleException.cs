using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Domain.Exceptions;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
[SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
public class RequestHandleException : Exception
{
	public HttpStatusCode HttpStatusCode { get; }
	public string Reason { get; }
	public string Note { get; }

	public RequestHandleException(HttpStatusCode httpStatusCode, string reason, string note) : base(reason)
	{
		HttpStatusCode = httpStatusCode;
		Reason = reason;
		Note = note;
	}

	public RequestHandleException(Exception? innerException, HttpStatusCode httpStatusCode, string reason, string note) : base(reason, innerException)
	{
		HttpStatusCode = httpStatusCode;
		Reason = reason;
		Note = note;
	}
}