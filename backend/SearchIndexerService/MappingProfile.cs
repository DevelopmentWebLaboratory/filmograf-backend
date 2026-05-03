using AutoMapper;
using Filmograf.BaseLibrary.Models.Repo;
using Filmograf.BaseLibrary.Models.SearchIndexes;

namespace Filmograf.SearchIndexerService;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CollectionRepo, CollectionCache>();
        
        CreateMap<MovieRepo, MovieSearchIndex>()
            .ForMember(dest => dest.NameSuggest, opt => 
                    opt.MapFrom(src => src.Name));
    }
}