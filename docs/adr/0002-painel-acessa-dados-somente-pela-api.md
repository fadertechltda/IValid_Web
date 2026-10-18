# 0002. Painel acessa dados somente pela API, com chave interna

- **Status:** Aceita
- **Data:** não registrada

## Contexto

O painel e o aplicativo precisam das mesmas regras (preço, estoque, pedidos). Se cada um acessasse o banco, as regras ficariam duplicadas. Também era preciso impedir que qualquer pessoa chamasse as rotas administrativas da API.

## Decisão

O painel (`WEB`) não acessa o Firestore nem o Firebase Auth: toda operação passa pela `WEB_API`. As rotas do painel exigem o cabeçalho `X-Internal-Api-Key`, com um segredo compartilhado só entre os dois serviços. As rotas do app (`api/app/*`) usam o token do Firebase.

## Consequências

- Uma única implementação das regras de negócio.
- O painel pode ser trocado ou reescrito sem mexer em regras.
- A API confia no `supermercadoId` enviado pelo painel. Quem tem a chave tem acesso a todas as lojas, então ela precisa ficar só no servidor.
- Uma evolução natural é a API emitir um token por usuário no login e validar a loja ela mesma.
