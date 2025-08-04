using AuthApi.Models;
using AutoMapper;


namespace AuthApi
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(cfg =>
            {
               cfg.CreateMap<User, UserDb>().ReverseMap();
            });
            return mappingConfig;
        }
    }
}
