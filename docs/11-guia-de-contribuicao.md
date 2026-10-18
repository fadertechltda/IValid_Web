# 11. Guia de contribuição

## Convenções de código

| Tema | Convenção | Exemplo |
|---|---|---|
| Idioma | Português em classes, métodos, variáveis e mensagens | `ListarPorSupermercadoAsync`, `PrecoPromocao` |
| Sufixos | O papel da classe vai no nome | `ProdutoModel`, `ProdutoValidacao`, `ProdutoMapeador`, `ProdutoProcesso`, `ProdutoFachada`, `ProdutoController`, `StatusPedidoUtil`, `PagamentoOpcoes` |
| Comentários | O código não usa comentários. Nomes devem explicar a intenção; explicações maiores vão para esta documentação ou para um ADR | |
| Construtores | *Primary constructors* com campo `_privado` | `public class OngProcesso(IOngMapeador ongMapeador)` |
| Coleções | *Collection expressions* | `return [.. lista];`, `Permissoes = []` |
| Nulos | `Nullable` habilitado em todos os projetos. Trate `null` explicitamente | |
| Erros de negócio | Lance `IValidExcecao(CodigoExcecao.X, "mensagem para o usuário")`. A mensagem aparece na tela, então escreva para o usuário final | |
| Campos do Firestore | Sempre com `[FirestoreProperty("nome")]`. Não renomeie campos existentes | |
| Datas | Grave em UTC. "Hoje" para regras diárias usa `America/Sao_Paulo` | |
| Fim de linha | Mantenha o do arquivo que está editando (o repositório tem CRLF e LF) | |

### Controllers da API

Controllers novos devem herdar de `ControllerApiBase` (painel) ou `ControllerAppBase` (app) e envolver a ação em `ExecutarAsync`. Assim o mapeamento de `CodigoExcecao` para status HTTP e o log de erro inesperado ficam padronizados:

```csharp
[Route("api/[controller]")]
public class OngController(OngFachada ongFachada) : ControllerApiBase
{
    private readonly OngFachada _ongFachada = ongFachada;

    [HttpGet]
    public Task<IActionResult> Listar() => ExecutarAsync(async () => Ok(await _ongFachada.ListarAsync()));
}
```

### Validação

Validadores herdam de `ValidadorAbstrato<T>` e separam as regras em `AssineRegrasInclusao`, `AssineRegrasAtualizacao` e `AssineRegrasExclusao`. A fachada chama o `Assine...` certo e depois `ValidateAsync`.

Cuidado: os validadores são registrados como `Scoped` e cada `Assine...` **adiciona** regras à instância. Chamar dois `Assine...` na mesma requisição soma as regras dos dois.

## Como adicionar uma funcionalidade (passo a passo)

Exemplo: um cadastro novo, "Fornecedor", por loja.

1. **Model** em `DOMAIN/Model/Fornecedor/FornecedorModel.cs`, com `[FirestoreData]`, `[FirestoreDocumentId] Id`, `[FirestoreProperty]` em cada campo e `SupermercadoId`.
2. **Validador** em `DOMAIN/Validador/Fornecedor/FornecedorValidacao.cs`, herdando de `ValidadorAbstrato<FornecedorModel>`.
3. **Mapeador** em `REPOSITORY/Mapeadores/Fornecedor/`: interface `IFornecedorMapeador` e implementação `FornecedorMapeador(FirestoreDb firestoreDb)`, com o nome da coleção em uma constante.
4. **Processo** em `SERVICE/Processo/FornecedorProcesso.cs`: regras e conferência de posse (`SupermercadoId`).
5. **Fachada** em `SERVICE/Fachada/`: valida e chama o processo.
6. **Injeção de dependência** em `WEB_API/Program.cs`: `AddScoped` do mapeador (pela interface), do processo, da fachada e do validador.
7. **Controller da API** em `WEB_API/Controllers/FornecedorController.cs`, herdando de `ControllerApiBase`, recebendo `supermercadoId` por query.
8. **Permissão** (se for uma área nova do painel): adicione a constante e a descrição em `PermissaoUtil` (`DOMAIN/Model/Perfil/PermissaoUtil.cs`). A *policy* é criada automaticamente.
9. **Controller do painel** em `WEB/Controllers/`, com `[Authorize(Policy = PermissaoUtil.Fornecedores)]`, lendo o `SupermercadoId` da claim e usando `CadastroApiHelper`.
10. **View** em `WEB/Views/Fornecedor/Index.cshtml`, com `Layout = "~/Views/Shared/_AdminLayout.cshtml"` e `ViewData["ActivePage"]`.
11. **Menu** em `_AdminLayout.cshtml`, dentro de `@if (User.TemPermissao(...))`.
12. **Documentação:** atualize [Referência da API](05-api-referencia.md), [Painel web](06-painel-web.md), [Modelo de dados](08-modelo-de-dados.md) e, se houver regra, [Regras de negócio](07-regras-de-negocio.md).

### Rota nova para o app

Coloque em `WEB_API/Controllers/App/`, com `[Route("api/app/...")]`, herdando de `ControllerAppBase`. Use `[Authorize]` (ou `[AllowAnonymous]` para leitura pública) e pegue o usuário de `UsuarioId`, nunca do corpo. O middleware da chave interna já ignora `api/app/*`.

### Cadastro compartilhado entre lojas

Se o dado vale para todas as lojas (como categorias e ONGs), a tela é do portal da plataforma: `[Authorize(Roles = PermissaoUtil.PerfilPlataforma)]` e link no bloco `User.EhPlataforma()` do menu. Veja o [ADR 0006](adr/0006-portal-da-plataforma.md).

## Testes

Não há projeto de testes automatizados. Antes de abrir um pull request:

1. `dotnet build IValid.slnx` sem erros novos.
2. Rode API e painel localmente e teste o fluxo alterado com pelo menos dois perfis (ex.: administrador e um funcionário sem a permissão).
3. Se mexeu em rota do app, teste com o `WEB_API.http` ou pelo Scalar.

Sugestão para quem quiser começar a suíte: xUnit para as regras puras (`ProdutoProcesso.AplicarRegras`, `StatusPedidoUtil`, `NivelFidelidadeUtil`, `SenhaHasher`), que não dependem do banco, e o emulador do Firestore para os processos.

## Git

- Repositório: `https://github.com/fadertechltda/IValid_Web`.
- Branch principal: `master`. Cada dev trabalha na própria branch (ex.: `dev-David`) e abre pull request para `master`.
- Mensagens no formato *Conventional Commits*, em português: `feat(api): rotas do aplicativo mobile`, `fix(web): ...`, `docs: ...`.
- Configure seu nome e e-mail **só neste repositório** (`git config user.email ...`, sem `--global`) se usar outra conta do GitHub no trabalho, para não publicar commits com a conta errada.
- Nunca faça commit de `Chave/`, de senhas ou de chaves de produção.
