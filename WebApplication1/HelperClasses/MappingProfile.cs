using AutoMapper;
using WebApplication1.Models;
using WebApplication1.Models.MyModels.Request;
using WebApplication1.Models.MyModels.Response;

namespace WebApplication1.HelperClasses
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegisterRequest, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
                .ForMember(dest => dest.DateJoined, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<User, TeacherModelForStudent>();
            
            CreateMap<User, EvaluationResponseModel>()
                .ForMember(dest => dest.Criterias, opt => opt.Ignore()); 

            CreateMap<Criterion, CriterionModel>(); 
            CreateMap<User, UserGetResponse>(); 

        }
    }
}
