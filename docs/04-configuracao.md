# 04. Configuração

O ASP.NET Core lê, nesta ordem (o último vence): `appsettings.json`, `appsettings.{Ambiente}.json` e variáveis de ambiente. Em variáveis de ambiente, `:` vira `__` (dois sublinhados). Exemplo: `Seguranca:ApiKeyInterna` vira `Seguranca__ApiKeyInterna`. Itens de lista usam o índice: `Plataforma__EmailsAdministradores__0`.

## WEB_API

| Chave | Obrigatória | Padrão | Uso |
|---|---|---|---|
| `Seguranca:ApiKeyInterna` | Sim | nenhum | Valor que o painel envia em `X-Internal-Api-Key`. Sem ela, toda rota fora de `api/app/*` responde `500`. |
| `Firebase:ProjectId` | Sim | `ivalid-9396b` | Projeto do Firestore e emissor esperado do token do app. |
| `Firebase:CredentialsPath` | Sim (exceto com emulador) | `../Chave/Chave_Firebase.json` | Caminho do JSON da conta de serviço, relativo à pasta da API. Vira `GOOGLE_APPLICATION_CREDENTIALS`. |
| `Firebase:ApiKey` | Sim | valor do projeto | *Web API key* do Firebase, usada nas chamadas REST de login, cadastro, redefinição de senha e exclusão de usuário. |
| `Email:ResendApiKey` | Para enviar e-mail | vazio | Chave de API do Resend (permissão *Sending access*). Defina só por variável de ambiente (`Email__ResendApiKey`), nunca no Git. Vazia: o cadastro funciona, mas o código de acesso não é enviado (aviso no log). |
| `Email:Remetente` | Para enviar e-mail | `no-reply@mail.ivalid.com.br` | Endereço remetente. Precisa pertencer a um domínio verificado no Resend. |
| `Email:NomeRemetente` | Não | `IValid` | Nome exibido. |
| `Pagamento:InfinitePayHandle` | Para pagamento online | `lucas-freitas-fe` | InfiniteTag da conta que recebe. Sem ela, gerar link de pagamento falha. |
| `Pagamento:InfinitePayBaseUrl` | Não | `https://api.checkout.infinitepay.io` | URL da API do InfinitePay. |
| `Pagamento:RedirectUrl` | Não | `https://ivalid.invalid/pagamento-concluido` | Para onde o checkout do InfinitePay redireciona depois do pagamento. O retorno traz `transaction_nsu` e `slug`, que o app envia em `pagamento/confirmar`. |
| `Pagamento:PermitirDemonstracao` | Não | `false` | Libera a forma `Demonstracao` (pedido pago sem cobrança). Use só em apresentação. |
| `Pagamento:MinutosExpiracaoPedidoPendente` | Não | `30` | Prazo para pagar um pedido online antes de ser cancelado. |
| `MercadoPago:AccessToken` | Para pagamento in-app | _(vazio)_ | Access Token da conta que recebe (cartão e Pix). **Informe somente por variável de ambiente** (`MercadoPago__AccessToken`), nunca no arquivo. |
| `MercadoPago:BaseUrl` | Não | `https://api.mercadopago.com/` | URL da API do Mercado Pago. |
| `Repasse:ComissaoPercentual` | Não | `1.49` | Comissão do IValid sobre cada pedido pago online (1.49 = 1,49%), descontada do repasse à loja. |
| `Notificacoes:Habilitada` | Não | `true` | Liga/desliga o envio de push. |
| `Notificacoes:TopicoOfertas` | Não | `ofertas` | Tópico FCM das ofertas diárias. O app precisa assinar o mesmo nome. |
| `Rotinas:Habilitada` | Não | `true` | Liga/desliga o `RotinaDiariaService`. |
| `Rotinas:IntervaloMinutos` | Não | `15` | Intervalo entre execuções (mínimo 1). |
| `Plataforma:EmailsAdministradores` | Para o portal | `[]` | E-mails que podem entrar no portal da plataforma. Lista vazia: ninguém entra (a API registra um aviso ao iniciar). |

Variáveis de ambiente extras:

| Variável | Uso |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` habilita `/openapi` e `/scalar`. Os Dockerfiles definem `Production`. |
| `PORT` | Porta HTTP no contêiner (padrão `10000`). O Render define sozinho. |
| `FIRESTORE_EMULATOR_HOST` | Ex.: `127.0.0.1:8080`. Faz a API usar o emulador do Firestore. |

## WEB

| Chave | Obrigatória | Padrão | Uso |
|---|---|---|---|
| `ApiSettings:BaseUrl` | Sim | `https://localhost:7183/` | URL da API, com `/` no final. |
| `Seguranca:ApiKeyInterna` | Sim | nenhum | Mesmo valor configurado na API. |

## Onde fica cada valor

| Ambiente | Onde configurar |
|---|---|
| Desenvolvimento | `appsettings.Development.json` de cada projeto e `Chave/Chave_Firebase.json` |
| Render | Variáveis de ambiente do serviço e *Secret File* para a credencial (veja [Deploy](10-deploy.md)) |

Não coloque senhas reais em `appsettings.json`: ele é versionado. Veja [Segurança](09-seguranca.md#segredos).
