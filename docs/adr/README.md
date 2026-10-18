# Registros de decisão de arquitetura (ADR)

Cada arquivo registra uma decisão: o contexto, o que foi decidido e as consequências. Formato de [Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions). Mais exemplos em [adr.github.io](https://adr.github.io/).

| Nº | Decisão | Status |
|---|---|---|
| [0001](0001-firestore-como-banco.md) | Firestore como banco de dados | Aceita |
| [0002](0002-painel-acessa-dados-somente-pela-api.md) | Painel acessa dados somente pela API, com chave interna | Aceita |
| [0003](0003-login-por-codigo-da-loja.md) | Login por código da loja; funcionários sem conta no Firebase | Aceita |
| [0004](0004-perfis-de-permissao-por-loja.md) | Perfis de permissão configuráveis por loja | Aceita |
| [0005](0005-preco-calculado-pela-api.md) | Preço, estoque e pagamento decididos pela API | Aceita |
| [0006](0006-portal-da-plataforma.md) | Portal da plataforma para dados compartilhados | Aceita |
| [0007](0007-beneficios-somente-na-finalizacao.md) | Benefícios do cliente só na finalização do pedido | Aceita |

## Como criar um ADR

1. Copie o [modelo](0000-modelo.md) com o próximo número.
2. Escreva em poucos parágrafos. O leitor é alguém daqui a um ano que vai perguntar "por que isso é assim?".
3. Adicione a linha na tabela acima.
4. Não edite ADRs aceitos. Se a decisão mudou, crie outro e marque o antigo como "Substituída por 00XX".
