using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.Dtos;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/fornecedores")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _fornecedorRepository; 
        private readonly IMapper _mapper;
        private readonly IFornecedorService _fornecedorService;
        private readonly IEnderecoRepository _enderecoRepository;

        public FornecedoresController(IFornecedorRepository fornecedorRepository, IMapper mapper,
        IFornecedorService fornecedorService, IEnderecoRepository enderecoRepository, INotificador _notificador) : base(_notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _enderecoRepository = enderecoRepository;
        }

        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<FornecedorDto>> ObterTodos()
        {
            var fornecedores = _mapper.Map<IEnumerable<FornecedorDto>>(await _fornecedorRepository.ObterTodos());
            return fornecedores;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> ObterPorId(Guid id)
        {
            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if(fornecedor is null) return NotFound();
            return Ok(fornecedor);
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> Adicionar(FornecedorDto fornecedorDto)
        {
            if(!ModelState.IsValid) return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorDto);
            await _fornecedorService.Adicionar(fornecedor);

            return CustomResponse(fornecedorDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> Atualizar(Guid id, FornecedorDto fornecedorDto)
        {
            if(id != fornecedorDto.Id) return BadRequest();

            if(!ModelState.IsValid) return CustomResponse(ModelState);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorDto);
            await _fornecedorService.Atualizar(fornecedor);

            return CustomResponse(fornecedorDto); 
        }

        [HttpDelete]
        public async Task<ActionResult<FornecedorDto>> Remover(Guid id)
        {
            var fornecedor = _mapper.Map<Fornecedor>(await _fornecedorRepository.ObterFornecedorEndereco(id));

            if(fornecedor is null) return NotFound();

            await _fornecedorService.Remover(fornecedor.Id);

            return CustomResponse(); 
        }

        [HttpGet("obter-endereco{id:guid}")]
        public async Task<EnderecoDto> ObterEndereoPorId(Guid id)
        {
            var enderecoDto = _mapper.Map<EnderecoDto>(await _enderecoRepository.ObterPorId(id));
            return enderecoDto;
        }

        [HttpGet("atualizar-endereco{id:guid}")]
        public async Task<IActionResult> AtualizarEndereco(Guid id, EnderecoDto enderecoDto)
        {
            if (id != enderecoDto.Id) return BadRequest();

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var endereco = _mapper.Map<Endereco>(enderecoDto);
            await _fornecedorService.AtualizarEndereco(endereco);

            return CustomResponse(enderecoDto);
        }

        public async Task<FornecedorDto> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorRepository.ObterPorId(id));
        }
    }
}