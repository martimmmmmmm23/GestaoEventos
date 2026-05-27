// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using GestãoEventos.Data;
using GestãoEventos.Data.Classes;
using GestãoEventos.Models;
using GestãoEventos.ViewModel.Participantes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace GestãoEventos.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        private readonly GestaoEventosDbContext _context;

        public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        GestaoEventosDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "O campo Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Por favor, introduza um endereço de email válido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "O campo Palavra-passe é obrigatório.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar palavra-passe")]
            [Compare("Password", ErrorMessage = "A palavra-passe e a confirmação não coincidem.")]
            public string ConfirmPassword { get; set; }
            
            [Required(ErrorMessage = "O campo Nome Completo é obrigatório.")]
            [Display(Name = "Nome Completo")]
            public string NomeCompleto { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.NomeCompleto = Input.NomeCompleto;

                //Cria os dados de Autenticação (Identity)
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Utilizador criou uma conta com password.");

                    //Grava o Participante na Base de Dados
                    var participante = new Participante();

                    participante.Nome = Input.NomeCompleto;
                    participante.Email = Input.Email;

                    _context.Add(participante);
                    await _context.SaveChangesAsync();

                    await _userManager.AddToRoleAsync(user, "Utilizador");

                    // 3. Faz o Login Imediato e manda o utilizador para a página inicial
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                //Se a pass for fraca ou o email já existir dá erro
                foreach (var error in result.Errors)
                {
                    string mensagemTraduzida = error.Description;

                    // Traduz o erro de email/utilizador já duplicado
                    if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                    {
                        mensagemTraduzida = "Este endereço de email já se encontra registado.";
                    }
                    // Traduz erro de password sem letras maiúsculas
                    else if (error.Code == "PasswordRequiresUpper")
                    {
                        mensagemTraduzida = "A palavra-passe deve conter pelo menos uma letra maiúscula ('A'-'Z').";
                    }
                    // Traduz erro de password sem algarismos
                    else if (error.Code == "PasswordRequiresDigit")
                    {
                        mensagemTraduzida = "A palavra-passe deve conter pelo menos um algarismo ('0'-'9').";
                    }
                    // Traduz erro de password sem caracteres especiais (ex: !, @, #)
                    else if (error.Code == "PasswordRequiresNonAlphanumeric")
                    {
                        mensagemTraduzida = "A palavra-passe deve conter pelo menos um carácter especial (ex: !, ?, @, #).";
                    }
                    ModelState.AddModelError(string.Empty, mensagemTraduzida);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Não foi possível criar uma instância de '{nameof(ApplicationUser)}'. " +
                    $"Garanta que a classe não é abstrata e possui um construtor sem parâmetros.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("O sistema de autenticação configurado requer um suporte de armazenamento que aceite emails.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
