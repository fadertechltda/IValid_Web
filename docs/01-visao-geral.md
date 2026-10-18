# 01. Visão geral

## O problema

Supermercados descartam alimentos que vencem na prateleira. O IValid ajuda a vender esses produtos antes do vencimento: o sistema calcula quantos dias faltam para cada produto vencer, aplica um desconto progressivo e mostra as ofertas em um aplicativo para o consumidor. O cliente também pode comprar um produto para doar a uma ONG parceira.

O projeto é um Trabalho de Conclusão de Curso. Ele precisa funcionar de ponta a ponta para a apresentação, mas não está em produção comercial.

## As três partes do sistema

| Parte | Tecnologia | Repositório | Quem usa |
|---|---|---|---|
| Painel web (`WEB`) | ASP.NET Core MVC | este | Supermercados e equipe IValid |
| API (`WEB_API`) | ASP.NET Core Web API | este | O painel e o aplicativo |
| Aplicativo | Flutter (Android) | separado | Consumidores |

Os dados ficam no **Firebase Firestore**. A autenticação de donos de loja, da equipe IValid e de clientes do app usa o **Firebase Authentication**.

## Atores

| Ator | Onde entra | O que faz |
|---|---|---|
| **Administrador da loja** (dono) | Painel, com código da loja, nome e senha | Tudo da própria loja: produtos, pedidos, funcionários, perfis, descontos e dados da loja |
| **Funcionário** | Painel, com código da loja, nome e senha | O que o perfil dele permitir (produtos, pedidos, financeiro, configurações) |
| **Equipe IValid** (perfil `Plataforma`) | Painel, em `/Autenticacao/LoginPlataforma` | Lista de supermercados, categorias e ONGs, que são compartilhadas entre todas as lojas |
| **Cliente** | Aplicativo | Vê ofertas, favorita produtos, faz pedidos, doa para ONGs e acumula cashback |

## Escopo

**Dentro do escopo:**
- Cadastro de lojas pelo próprio dono (registro aberto).
- Cadastro de produtos com preço, custo, estoque, vencimento, marca, categoria e imagem (por URL).
- Cálculo automático de faixa de validade, desconto e preço promocional.
- Funcionários com perfis de permissão configuráveis por loja.
- Pedidos feitos pelo app, com endereço opcional e status de entrega (`EM_ROTA`), pagos em dinheiro, voucher, online (InfinitePay) ou com cashback.
- Doações para ONGs, cashback, níveis de fidelidade e indicadores de impacto.
- Notificações push (Firebase Cloud Messaging).

**Fora do escopo, por enquanto:**
- Estorno automático de pagamento (a loja faz manualmente; o sistema só sinaliza `estornoPendente`).
- Repasse de valores para a loja (split). Todo pagamento online cai em uma única conta InfinitePay.
- Bloqueio de lojas pela plataforma.
- Testes automatizados no repositório.

## Glossário

| Termo | Significado |
|---|---|
| **Loja** / **Supermercado** | Unidade de isolamento dos dados. Documento da coleção `supermercados`. Quase tudo carrega um `supermercadoId`. |
| **Código de acesso** | Identificador da loja usado no login, gerado a partir do nome (ex.: `mercadobompreco`). Enviado por e-mail no cadastro. |
| **Faixa de validade** / **Status** | `VERDE` (mais de 20 dias), `AMARELO` (11 a 20), `VERMELHO` (0 a 10) ou `VENCIDO`. Ver [regras de negócio](07-regras-de-negocio.md). |
| **Esgotado** | Produto com `quantidade <= 0`. |
| **Oferta relâmpago** | Produto na faixa `VERMELHO`. |
| **Item de doação** | Item do pedido com `origem = doacao`. O cliente paga e o produto vai para uma ONG. |
| **ONG** | Instituição que recebe as doações. Cadastro compartilhado, mantido pela equipe IValid. |
| **Cashback** | Saldo do cliente, ganho ao doar, que pode abater até 15% de um pedido. |
| **Nível de fidelidade** | Bronze, Prata ou Ouro, conforme o total de itens doados. Define o percentual de cashback. |
| **Impacto** | Totais do cliente: itens resgatados, itens doados, economia e valor doado. |
| **Perfil** | Conjunto de permissões de funcionário, por loja (ex.: Gerente, Atendente). |
| **Plataforma** | Perfil da equipe IValid, fora de qualquer loja. |
| **Chave interna** | Cabeçalho `X-Internal-Api-Key` que o painel envia para a API. |
| **Fachada** | Classe da camada `SERVICE` que orquestra validação e processos de um caso de uso. |
| **Processo** | Classe da camada `SERVICE` com a regra de negócio. |
| **Mapeador** | Classe da camada `REPOSITORY` que lê e grava no Firestore. |
| **RN01** | Regra de preço por faixa de validade, citada nos documentos do TCC. |
