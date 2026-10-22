# 05. Referência da API

A API tem dois públicos, com autenticações diferentes:

| Grupo | Prefixo | Quem chama | Autenticação |
|---|---|---|---|
| Painel | `api/{Recurso}` | Somente a `WEB` | Cabeçalho `X-Internal-Api-Key` |
| Aplicativo | `api/app/*` | App Flutter | `Authorization: Bearer <idToken do Firebase>` (exceto a vitrine, que é pública) |

Em desenvolvimento, a documentação interativa (OpenAPI + Scalar) fica em `/scalar` e o JSON em `/openapi/v1.json`. Em produção essas rotas não existem.

O corpo das requisições e respostas é JSON. Os nomes das propriedades seguem os models C# em *camelCase* (ex.: `supermercadoId`, `informacaoAdicional`).

## Erros

Toda resposta de erro tem o formato:

```json
{ "codigo": 2, "informacaoAdicional": "Mensagem para mostrar ao usuário." }
```

`codigo` é o número do enum `CodigoExcecao`:

| Código | Nome | Status HTTP nas rotas com `ControllerApiBase` |
|---|---|---|
| 0 | `Generico` | 500 |
| 1 | `ValidacaoSeguranca` | 400 |
| 2 | `SolicitarValidacao` | 400 |
| 3 | `Concorrencia` | 409 |
| 4 | `EntidadeNaoEncontrada` | 404 |
| 5 | `NaoAutorizado` | 403 |
| 6 | `EstoqueInsuficiente` | 409 |
| 7 | `PagamentoNaoConfirmado` | 409 |
| 8 | `TransicaoStatusInvalida` | 409 |

Os controllers mais antigos (`Produto`, `Pedido`, `Funcionario`, `Configuracao`, `Dashboard`, `Usuario`) não herdam de `ControllerApiBase` e respondem **400 para qualquer erro**, com o mesmo corpo. Os novos (`Categoria`, `Ong`, `Perfil`, `Supermercado` e todos de `api/app`) seguem a tabela acima.

Quando um pedido tem itens indisponíveis, a resposta é `409` com um campo a mais, `resumo`, igual ao de `pedidos/validar`, com a lista `problemas`.

Outras respostas possíveis: `401` sem token ou com chave interna errada, e `400` de validação automática do ASP.NET (`ProblemDetails` com `errors`) quando o corpo não respeita os atributos `[Required]`, `[Range]` etc.

---

## Rotas do painel

Todas exigem `X-Internal-Api-Key`. As que recebem `supermercadoId` usam esse valor para filtrar e para conferir se o recurso pertence à loja. Quem garante que o `supermercadoId` é o do usuário logado é a `WEB`, que o lê do cookie.

### Usuário e login (`api/Usuario`)

| Método | Rota | Corpo / query | Resposta |
|---|---|---|---|
| GET | `usuarios-loja?codigoLoja=` | | `[{ chave, nome }]`. `chave` é `ADMIN:{id}` ou `FUNC:{id}`. Só funcionários ativos. |
| POST | `login-loja` | `{ codigoLoja, usuarioChave, senha }` | `ResultadoLoginModel`: `{ id, nome, email, perfil, nomePerfil, supermercadoId, permissoes[] }` |
| POST | `login-plataforma` | `{ email, senha }` | `ResultadoLoginModel` com `perfil = "Plataforma"`. Erro genérico "Email ou senha inválidos." para e-mail fora da lista ou senha errada. |
| POST | `registro` | `RegistroModel`: `{ nomeCompleto, email, senha, confirmarSenha, nomeSupermercado, cnpjSupermercado, enderecoSupermercado }` | `{ codigoAcesso }`. Cria usuário no Firebase Auth, a loja e o administrador. Se a gravação no Firestore falhar, exclui o usuário criado no Firebase. |
| POST | `esqueci-senha` | `{ email }` | Sempre `200`, exista ou não o e-mail (evita descobrir contas). |

### Produtos (`api/Produto`)

| Método | Rota | Corpo / query | Resposta |
|---|---|---|---|
| GET | `?supermercadoId=` | | Lista com status, desconto e preço recalculados, ordenada (veja [regras](07-regras-de-negocio.md#ordem-da-listagem-no-painel)). |
| GET | `{id}?supermercadoId=` | | Produto ou `404`. |
| POST | `?supermercadoId=` | `ProdutoModel` | `201`. |
| PUT | `{id}?supermercadoId=` | `ProdutoModel` com `id` | `204`. |
| DELETE | `{id}?supermercadoId=` | | `200`. |

### Pedidos (`api/Pedido`)

| Método | Rota | Corpo / query | Resposta |
|---|---|---|---|
| GET | `?supermercadoId=` | | Pedidos da loja, com nome do cliente. Inclui pedidos antigos associados pelos produtos. |
| GET | `{id}?supermercadoId=` | | Pedido ou `404`. |
| PATCH | `{id}/status?supermercadoId=` | `{ status }` | Pedido atualizado. Só aceita transições permitidas e envia push ao cliente. |

### Demais rotas do painel

| Método | Rota | Uso |
|---|---|---|
| GET | `api/Dashboard?supermercadoId=` | `DashboardModel`: contagens por faixa, valor em risco, 5 produtos urgentes, faturamento e lucro. |
| GET | `api/Repasse?supermercadoId=` | `ResumoRepasseModel`: total a repassar e fechamentos semanais (segunda a domingo, UTC-3) com vendas brutas, comissão, valor líquido, pagamento previsto (quarta seguinte) e pedidos. Entram só pedidos pagos online, `FINALIZADO` e sem estorno pendente. |
| GET / PUT | `api/Configuracao?supermercadoId=` / `api/Configuracao` | Percentuais de desconto da loja (`percentualDescontoAmarelo`, `percentualDescontoVermelho`). |
| GET | `api/Funcionario?supermercadoId=` | Funcionários da loja, com o nome do perfil. |
| GET | `api/Funcionario/{id}?supermercadoId=` | Um funcionário. |
| POST | `api/Funcionario?supermercadoId=` | `{ nome, senha, confirmarSenha, perfilId }`. |
| PUT | `api/Funcionario/{id}?supermercadoId=` | `{ id, nome, perfilId, ativo, novaSenha? }`. |
| GET | `api/Perfil?supermercadoId=` | Perfis da loja. Cria os três padrões na primeira chamada. |
| GET | `api/Perfil/permissoes` | Catálogo de permissões (`PermissaoUtil.Todas`). |
| GET / POST / PUT / DELETE | `api/Perfil[/{id}]?supermercadoId=` | CRUD de perfis. |
| GET | `api/Supermercado` | Resumo de todas as lojas (portal da plataforma). |
| GET / PUT | `api/Supermercado/{id}` | Dados da loja: `{ nome, endereco, latitude, longitude }`. Ao mudar o nome, atualiza `storeName` dos produtos. |
| GET / POST / PUT / DELETE | `api/Categoria[/{id}]` | Categorias (compartilhadas). |
| GET / POST / PUT / DELETE | `api/Ong[/{id}]` | ONGs (compartilhadas). |

---

## Rotas do aplicativo

O app obtém o token com `FirebaseAuth.instance.currentUser!.getIdToken()`. A identidade é o `user_id` do token.

### Vitrine (pública, `api/app`)

| Método | Rota | Descrição |
|---|---|---|
| GET | `produtos` | Vitrine paginada. Não mostra vencidos, esgotados nem produtos sem loja. Preço e dias calculados na hora. |
| GET | `produtos/ofertas-relampago` | Mesma vitrine, só faixa `VERMELHO`. |
| GET | `produtos/{id}` | Detalhe, inclusive de vencidos e esgotados. |
| GET | `categorias` | Categorias. |
| GET | `supermercados` e `supermercados/{id}` | `{ id, nome, endereco, latitude, longitude }`. |
| GET | `ongs` | Somente ONGs ativas. |

Filtros de `produtos` (query): `categoriaId`, `busca`, `supermercadoId`, `status`, `ordem` (`vencimento`, `menorPreco`, `maiorDesconto`, `nome`), `pagina` (a partir de 1) e `tamanhoPagina` (1 a 100, padrão 20).

Resposta paginada:

```json
{
  "itens": [
    {
      "id": "abc", "nome": "Iogurte", "marca": "X", "urlImagem": "https://...",
      "categoriaId": "c1", "supermercadoId": "s1", "nomeSupermercado": "Mercado",
      "precoOriginal": 10.0, "precoAtual": 6.0, "percentualDesconto": 40,
      "diasParaVencer": 3, "dataVencimento": "2026-10-21T00:00:00Z",
      "status": "VERMELHO", "quantidadeDisponivel": 5
    }
  ],
  "pagina": 1, "tamanhoPagina": 20, "total": 42, "temProxima": true
}
```

### Cliente (`api/app/clientes`, exige token)

| Método | Rota | Corpo | Descrição |
|---|---|---|---|
| POST | `/` | `{ nomeCompleto, telefone? }` | Cria o perfil do cliente logado. Idempotente. |
| GET | `me` | | `PerfilClienteModel`: dados, nível, percentual de cashback, doações para o próximo nível e impacto. |
| PUT | `me` | `{ nomeCompleto, telefone?, endereco? }` | Atualiza o perfil. |
| GET | `me/impacto` | | Totais de impacto. |
| GET | `me/favoritos` | | Produtos favoritos (formato da vitrine). |
| PUT / DELETE | `me/favoritos/{produtoId}` | | Adiciona / remove. `204`. |
| POST | `me/dispositivos` | `{ token }` | Registra o token FCM. `204`. |
| POST | `me/dispositivos/remover` | `{ token }` | Remove o token (logout). `204`. |

Uma conta do painel (documento em `users` com `supermercadoId`) recebe `403` nessas rotas.

### Pedidos (`api/app/pedidos`, exige token)

| Método | Rota | Corpo | Descrição |
|---|---|---|---|
| POST | `validar` | `CriarPedidoModel` | Simula sem gravar: preços, cashback aplicável, total e `problemas`. |
| POST | `/` | `CriarPedidoModel` | Cria o pedido. `201` com o pedido. |
| GET | `/` e `{id}` | | Pedidos do cliente. |
| POST | `{id}/cancelar` | | Só em `PENDENTE` ou `CONFIRMADO`. Devolve estoque e cashback. |
| POST | `{id}/pagamento` | `{ metodo: "pix" \| "cartao", tokenCartao?, bandeiraId?, tipoCartao?, parcelas? }` | Cria a cobrança no Mercado Pago. Pix devolve `qrCode`, `qrCodeBase64` e `ticketUrl`; cartão usa o token gerado no app. Marca como pago só após consultar a ordem no Mercado Pago. |
| POST | `{id}/pagamento/sincronizar` | `{ ordemId }` | Reconsulta a ordem no Mercado Pago (usado após o Pix). |
| POST | `/api/app/pagamentos/mercadopago/webhook` | notificação do Mercado Pago | Anônimo. Só o id da ordem é lido; o estado real é consultado na API do Mercado Pago. |
| POST | `{id}/pagamento/link` | | Gera o link do InfinitePay (pedido `Online` em `PENDENTE`). Retorna `{ pedidoId, url, valor }`. |
| POST | `{id}/pagamento/confirmar` | `{ transactionNsu, slug, captureMethod?, receiptUrl? }` | A API consulta o InfinitePay e só então marca como pago. |

`CriarPedidoModel`:

```json
{
  "formaPagamento": "Online",
  "cashbackUsado": 2.5,
  "ongId": "ong1",
  "endereco": "Rua A, 10",
  "itens": [
    { "produtoId": "p1", "quantidade": 2, "origem": "compra" },
    { "produtoId": "p2", "quantidade": 1, "origem": "doacao" }
  ]
}
```

- `formaPagamento`: `Dinheiro`, `Voucher`, `Online`, `Cashback` ou `Demonstracao`.
- `origem`: `compra` (padrão) ou `doacao`.
- `endereco`: opcional. Se vier vazio, usa o endereço do perfil.
- Nunca envie preço: a API calcula.

As regras completas (limites, estados e benefícios) estão em [Regras de negócio](07-regras-de-negocio.md#pedidos).
