using Api.Extensions;
using AutoMapper;
using DevIO.Api.Dtos;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        public ProdutosController(INotificador notificador, IProdutoRepository produtoRepository,
            IProdutoService produtoService, IMapper mapper) : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoDto>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoDto>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> ObterPorId(Guid id)
        {
            var produtoDto = await ObterProduto(id);
            if (produtoDto == null) return NotFound();

            return produtoDto;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> Adicionar([ModelBinder(BinderType = typeof(ProdutoModelBinder))]  ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var nomeDaImagem = Guid.NewGuid() + "_";
            if (!await UploadArquivo(produtoDto.ImagemUpload, nomeDaImagem))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = nomeDaImagem;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> Excluir(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produto);
        }

        private async Task<ProdutoDto> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoDto>(await _produtoRepository.ObterProdutoFornecedor(id));
        }

        private async Task<bool> UploadArquivo(IFormFile arquivo, string imgNome)
        {
            if (arquivo is null || arquivo.Length <= 0)
            {
                NotificarErro("Forneça uma imagem para o produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imgNome + arquivo.FileName);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
    }
}
