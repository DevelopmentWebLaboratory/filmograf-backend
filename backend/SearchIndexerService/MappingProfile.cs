using AutoMapper;
using Filmograf.BaseLibrary.Models.Repo;

namespace Filmograf.SearchIndexerService;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CollectionRepo, CollectionCache>();
    }
}