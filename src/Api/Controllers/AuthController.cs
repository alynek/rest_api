using Api.Dtos;
using DevIO.Business.Intefaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api")]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager) : base(notificador)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
                return CustomResponse(register);
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

            if (result.Succeeded) return CustomResponse(login);

            if (result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse(login);
            }

            NotificarErro("Usuário ou senha inválidos");
            return CustomResponse(login);
        }
    }
}
