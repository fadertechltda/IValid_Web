# 10. Deploy

A API e o painel são publicados como dois serviços Docker independentes no [Render](https://render.com/): `ivalid-api` e `ivalid-web`.

## Imagens

| Serviço | Dockerfile | Projetos copiados | Saída |
|---|---|---|---|
| API | `Dockerfile.WebApi` | `DOMAIN`, `Excecoes`, `REPOSITORY`, `SERVICE`, `WEB_API` | `WEB_API.dll` |
| Painel | `Dockerfile.Web` | `DOMAIN`, `Excecoes`, `WEB` | `WEB.dll` |

As duas usam build em dois estágios (`sdk:10.0` para compilar, `aspnet:10.0` para rodar), definem `ASPNETCORE_ENVIRONMENT=Production` e escutam em `http://+:${PORT}` (padrão 10000). O Render define `PORT` e cuida do HTTPS na frente do contêiner. Os dois `Program.cs` já tratam os cabeçalhos `X-Forwarded-*`.

O `.dockerignore` deixa de fora `bin/`, `obj/`, `Chave/`, `appsettings.Development.json` e arquivos `.md`. Ou seja: a imagem **não** leva a credencial do Firebase nem a chave interna de desenvolvimento. Elas precisam vir do Render.

Para testar a imagem localmente:

```bash
docker build -f Dockerfile.WebApi -t ivalid-api .
docker run -p 10000:10000 -e Seguranca__ApiKeyInterna=teste \
  -e Firebase__CredentialsPath=/segredos/chave.json \
  -v "$PWD/Chave/Chave_Firebase.json:/segredos/chave.json:ro" ivalid-api
```

## Configuração no Render

### `ivalid-api`

| Tipo | Nome | Valor |
|---|---|---|
| Secret File | `Chave_Firebase.json` | Conteúdo da conta de serviço. O Render monta em `/etc/secrets/Chave_Firebase.json`. |
| Variável | `Firebase__CredentialsPath` | `/etc/secrets/Chave_Firebase.json` |
| Variável | `Seguranca__ApiKeyInterna` | Valor longo e aleatório, **diferente** do de desenvolvimento |
| Variável | `Plataforma__EmailsAdministradores__0` (`__1`...) | E-mails da equipe |
| Variável | `Email__EmailRemetente`, `Email__SenhaRemetente` | Conta SMTP (opcional) |
| Variável | `Pagamento__RedirectUrl`, `Pagamento__InfinitePayHandle` | Se forem diferentes do `appsettings.json` |

### `ivalid-web`

| Tipo | Nome | Valor |
|---|---|---|
| Variável | `ApiSettings__BaseUrl` | URL pública da API, com `/` no final (ex.: `https://ivalid-api.onrender.com/`) |
| Variável | `Seguranca__ApiKeyInterna` | O **mesmo** valor da API |

Confira esses valores no painel do Render antes de publicar: este documento descreve a configuração esperada pelo código, não uma cópia do que está cadastrado lá.

## Checklist de publicação

1. `dotnet build IValid.slnx` sem erros.
2. Mudou configuração? Atualize as variáveis no Render **antes** do deploy.
3. Publique a **API primeiro** e depois o painel, se houver mudança de contrato entre os dois.
4. Depois do deploy, teste:
   - `GET https://<api>/api/app/categorias` responde 200 (sem chave);
   - `GET https://<api>/api/Produto` sem chave responde 401;
   - login de uma loja no painel;
   - abrir Produtos e Pedidos.
5. Mudou regra de preço ou campos do Firestore? Avise quem mantém o app Flutter.

## Pontos de atenção

- **Plano gratuito:** o serviço dorme após um período sem acesso. A primeira requisição demora e as rotinas em segundo plano ficam paradas enquanto ele dorme. Para a apresentação, abra o painel e a API alguns minutos antes.
- **Sem health check dedicado:** se precisar, use `GET /api/app/categorias`, que é público e consulta o Firestore.
- **Uma instância só:** as rotinas usam a coleção `rotinas` para não repetir no mesmo dia, mas o cancelamento de pedidos expirados roda em toda instância. Se escalar para mais de uma, revise isso.
- **Rollback:** no Render, use "Rollback" para a última versão que funcionava. Mudanças de dados no Firestore não voltam junto.
