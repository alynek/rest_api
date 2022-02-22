using AutoMapper;
using DevIO.Api.Dtos;
using DevIO.Business.Models;

namespace Api.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Fornecedor, FornecedorDto>().ReverseMap();
            CreateMap<Endereco, EnderecoDto>().ReverseMap();
            CreateMap<Produto, ProdutoDto>().ReverseMap();
        }
    }
}