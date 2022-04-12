using Api.Dtos;
using Api.Extensions;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenJWT _tokenJWT;
        public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager, IOptions<TokenJWT> tokenJWT) : base(notificador)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenJWT = tokenJWT.Value;
        }

        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(RegisterUserDto register)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = register.Email,
                Email = register.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, register.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return CustomResponse(GerarJWT());
            }
            foreach(var erro in result.Errors)
            {
                NotificarErro(erro.Description);
            }

            return CustomResponse(register);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserDto login)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = login.Email,
                Email = login.Email
            };

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, true);

            if (result.Succeeded) return CustomResponse(GerarJWT());

            if (result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse(login);
            }

            NotificarErro("Usuário ou senha inválidos");
            return CustomResponse(login);
        }

        private string GerarJWT()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_tokenJWT.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _tokenJWT.Emissor,
                Audience = _tokenJWT.ValidoEm,
                Expires = DateTime.UtcNow.AddHours(_tokenJWT.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);
            return encodedToken;
        }
    }
}
