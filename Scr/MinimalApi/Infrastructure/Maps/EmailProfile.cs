using AutoMapper;
using Database.Models;
using MinimalApi.Models;

namespace MinimalApi.Infrastructure.Maps;

public class EmailProfile : Profile
{
	public EmailProfile()
	{
		CreateMap<RequestModel, EmailTbl>()
			.ForMember(dest => dest.TemplateId, opt => opt.Ignore())
			.ForMember(dest => dest.AttachementCount, opt => opt.MapFrom((src, _) => src.Attachments?.Count() ?? 0));

		CreateMap<EmailAddresses, EmailAddressTbl>();
		CreateMap<AttachementsModels, EmailAttachmentTbl>();
	}
}