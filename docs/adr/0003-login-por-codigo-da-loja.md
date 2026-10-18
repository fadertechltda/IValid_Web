# 0003. Login por código da loja; funcionários sem conta no Firebase

- **Status:** Aceita
- **Data:** não registrada

## Contexto

O sistema é multiloja, e cada loja tem o dono e vários funcionários. Usar o Firebase Auth para todos exigiria um e-mail por funcionário e daria ao Firebase o controle de contas que pertencem, na prática, à loja.

## Decisão

- Cada loja tem um **código de acesso** único, enviado por e-mail no cadastro.
- O login é em duas etapas: o código da loja e depois o usuário (escolhido numa lista) com a senha.
- O dono da loja autentica no Firebase Auth. O sistema descobre o e-mail pelo usuário escolhido.
- Funcionários são cadastrados pelo dono e autenticados pela API, com hash PBKDF2-SHA256 guardado no Firestore.

## Consequências

- O dono cadastra funcionários sem precisar de e-mail.
- Quem sabe o código da loja vê os nomes dos usuários dela.
- "Esqueci a senha" só serve para o dono; a senha do funcionário é trocada pelo dono.
- Uma pessoa com duas lojas tem duas contas.
