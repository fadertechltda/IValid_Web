# 12. Problemas conhecidos e dívida técnica

Lista do que já se sabe que não está ideal, com o motivo e a sugestão. Ao resolver um item, apague-o daqui no mesmo pull request.

## Desempenho

| Item | Onde | Por que importa | Sugestão |
|---|---|---|---|
| A vitrine carrega **todos** os produtos e filtra em memória | `VitrineProcesso.ListarProdutosAsync` | Cada acesso ao app lê a coleção inteira. Fica lento e caro com muitos produtos | Consultas no Firestore por `supermercadoId`/`categoryId` e paginação por cursor |
| Pedidos antigos são associados à loja lendo **todos** os pedidos sem `supermercadoId` e todos os produtos da loja | `PedidoProcesso.ListarPedidosLegadosDaLojaAsync` e `PertenceALojaAsync` | Toda abertura da tela de pedidos faz essa varredura | Migração única gravando `supermercadoId` nos pedidos antigos; depois, remover o código legado |
| Lista de supermercados da plataforma faz uma consulta por loja para achar o administrador | `SupermercadoFachada.ListarParaPlataformaAsync` | N+1 consultas | Aceitável com poucas lojas |

## Consistência e regras

| Item | Detalhe | Sugestão |
|---|---|---|
| Regra de preço duplicada no app Flutter | O app calcula desconto em Dart com o padrão 20/40 fixo. Mudou aqui, precisa mudar lá | App passar a usar só `api/app/produtos` |
| Coleção `users` mistura administradores de loja e clientes do app | Diferenciados só pela presença de `supermercadoId` | Mover administradores para `administradores` (exige migração) |
| Forma de pagamento `Cashback` quase inútil | O cashback cobre no máximo 15% do subtotal, então o total quase nunca zera | Revisar a regra ou tirar a opção do app |
| `ConfiguracaoValidacao` não exige desconto vermelho maior ou igual ao amarelo | A loja pode configurar o contrário | Adicionar a regra |
| Excluir ONG ou categoria apaga o documento | Pedidos e produtos ficam apontando para um id que não existe | Usar a opção "ativa" (ONGs) e bloquear exclusão de categoria em uso |
| `PUT api/Produto/{id}` usa o `id` do corpo e ignora o da rota | Confuso para quem chama | Usar o `id` da rota |
| Validação de vencimento usa `DateTime.Today` (hora do servidor), e o cálculo de dias usa UTC | No contêiner os dois são UTC; perto da meia-noite no Brasil pode haver um dia de diferença | Usar o fuso `America/Sao_Paulo` em ambos |
| Falha no envio do e-mail de cadastro é ignorada sem aviso | O dono da loja não recebe o código e não sabe por quê | Mostrar o código na tela de sucesso ou registrar log |

## Código e infraestrutura

| Item | Detalhe | Sugestão |
|---|---|---|
| Controllers antigos não usam `ControllerApiBase` | `Produto`, `Pedido`, `Funcionario`, `Configuracao`, `Dashboard`, `Usuario` respondem 400 para tudo; `Produto` e `Funcionario` expõem `ex.Message` | Migrar para `ExecutarAsync` |
| `appsettings.Development.json` está versionado | Contém a chave interna de desenvolvimento | Mover para *User Secrets* (`dotnet user-secrets`) e adicionar ao `.gitignore` |
| Projeto `IValid/` (com `Class1.cs`) sobrou do template | Não está na solução | Apagar a pasta |
| Pacote `H.Necessaire.Runtime.Google.FirestoreDB` em `WEB` e `WEB_API` | Nenhum código usa | Remover e testar o build |
| Avisos de API obsoleta | `ForwardedHeadersOptions.KnownNetworks` (use `KnownIPNetworks`) e `MulticastMessage.Tokens` | Atualizar |
| Aviso ASP0026 no `Logout` | `[Authorize]` anulado pelo `[AllowAnonymous]` da classe | Mover o `Logout` ou tirar o atributo |
| Sem testes automatizados | | Veja a sugestão em [Guia de contribuição](11-guia-de-contribuicao.md#testes) |
| Rotinas dependem da API estar acordada | No plano gratuito do Render ela dorme | Plano pago ou *cron job* externo chamando a API |

## Segurança

Os riscos estão em [Segurança](09-seguranca.md#riscos-conhecidos).
