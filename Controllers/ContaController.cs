// Controllers/ContaController.cs
using Microsoft.AspNetCore.Mvc;
using VigiLant.Services;
using VigiLant.Models;
using VigiLant.Models.Enum;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using VigiLant.Contratos;

public class ContaController : Controller
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHashService _hashService;
    private readonly IColaboradorRepository _colaboradorRepository;
    private readonly ISolicitacaoRepository _solicitacaoRepository;

    public ContaController(IUsuarioRepository usuarioRepository, IHashService hashService, IColaboradorRepository colaboradorRepository, ISolicitacaoRepository solicitacaoRepository)
    {
        _usuarioRepository = usuarioRepository;
        _hashService = hashService;
        _colaboradorRepository = colaboradorRepository;
        _solicitacaoRepository = solicitacaoRepository;
    }

    private IActionResult RedirecionarParaHome() => RedirectToAction("Index", "Home");

    // --- Perfil (GET) ---
    [HttpGet]
    public IActionResult Perfil()
    {

        ViewData["Title"] = "Perfil";
        return View();
    }

    // --- Login (GET) ---
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated) return RedirecionarParaHome();
        ViewData["Title"] = "Login";
        return View();
    }

    // --- Login (POST) ---
    [HttpPost]
    public async Task<IActionResult> Login(string email, string senha)
    {
        ViewData["Title"] = "Login";

        // 1. Buscar usuário
        var usuario = await _usuarioRepository.BuscarPorEmail(email);

        // 2. Validar credenciais
        if (usuario == null || !_hashService.VerificarHash(senha, usuario.SenhaHash))
        {
            ViewBag.Erro = "E-mail ou Senha inválidos.";
            return View();
        }

        // 3. Criar Claims (Identidade)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.cargo.ToString()) // Adiciona a Role/Cargo
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            // isPersistent: true para "Lembrar-me"
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        // 4. Logar o usuário (Cria o Cookie)
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirecionarParaHome();
    }

    // --- Register (GET) ---
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated) return RedirecionarParaHome();
        ViewData["Title"] = "Register";
        return View();
    }

    // --- Register (POST) ---
    [HttpPost]
    public async Task<IActionResult> Register(string nome, string email, string senha)
    {
        ViewData["Title"] = "Register";
        // 1. Verificar se o e-mail já foi usado para criar uma conta de usuário
        if (await _usuarioRepository.BuscarPorEmail(email) != null)
        {
            ViewBag.Erro = "Este e-mail já está cadastrado no sistema. Por favor, faça login.";
            return View();
        }

        // 2. Tentar encontrar o colaborador correspondente no banco de dados
        var colaborador = await _colaboradorRepository.GetByEmail(email);

        if (colaborador == null)
        {
            var novaSolicitacao = new Solicitacao
            {
                Nome = nome,
                Email = email,
                DataSolicitacao = DateTime.Now,
                Status = StatusSolicitacao.Pendente
            };

            await _solicitacaoRepository.Adicionar(novaSolicitacao);

            ViewBag.Erro = "Seu e-mail não consta em nossa base. Sua solicitação de participação foi enviada e será revisada por um administrador.";

            return View();
        }

        // 3. Verificar se o colaborador já está vinculado a uma conta de usuário
        if (colaborador.UsuarioId.HasValue)
        {
            // Esta verificação já é coberta pela busca no _usuarioRepository,
            // mas é um ponto de segurança: se ele tem UsuarioId, a conta já existe
            ViewBag.Erro = "O colaborador com este e-mail já está vinculado Por favor, use a tela de login.";
            return View();
        }

        // CASO 2: Colaborador ENCONTRADO e NÃO VINCULADO

        // 4. Criar novo usuário do sistema
        var novoUsuario = new Usuario
        {
            Nome = nome,
            Email = email,
            SenhaHash = _hashService.GerarHash(senha),
            cargo = colaborador.Cargo
        };

        // 5. Salvar o novo Usuário no DB
        await _usuarioRepository.Adicionar(novoUsuario);

        // 6. VINCULAR: Atualizar o registro do Colaborador com o novo UsuarioId e Status
        colaborador.UsuarioId = novoUsuario.Id;
        colaborador.StatusAcesso = StatusVinculacao.Ativo;
        _colaboradorRepository.UpdateStatusVinculacao(colaborador);

        // 7. Sucesso! Redireciona com mensagem de sucesso de vinculação
        TempData["Sucesso"] = "Registro realizado e conta vinculada ao seu perfil de colaborador com sucesso. Por favor, faça login.";
        return RedirectToAction("Login");
    }

    // --- Logout (POST) ---
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Remove o cookie de autenticação
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // CORREÇÃO: Volta para a tela de Login
        return RedirectToAction("Login");
    }
}