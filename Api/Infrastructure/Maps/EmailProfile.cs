using AutoMapper;
using Database.Models;
using Domain.Models;

namespace Api.Infrastructure.Maps;

public class EmailProfile : Profile
{
	public EmailProfile()
	{
		CreateMap<EmailModel, EmailTbl>()
			.ForMember(dest => dest.TemplateId, opt => opt.Ignore())
			.ForMember(dest => dest.Attachements, opt => opt.Ignore());

		CreateMap<EmailAddresses, EmailAddressTbl>();
	}
}