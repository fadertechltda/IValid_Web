# 0005. Preço, estoque e pagamento decididos pela API

- **Status:** Aceita
- **Data:** 2026-10-13

## Contexto

O app gravava pedidos direto no Firestore, com o preço e o status calculados no aparelho. Os campos derivados do produto (`newPrice`, `daysValidity`, `status`) ficavam congelados desde o cadastro, então o app podia vender com desconto errado ou até vender produto vencido. Um app modificado poderia mandar qualquer preço.

## Decisão

- A API recalcula status, desconto e preço em toda leitura e na criação do pedido.
- O pedido é criado numa transação que relê os produtos, confere estoque e validade, baixa o estoque e debita o cashback.
- O pagamento online só é dado como pago depois que a API consulta o InfinitePay.
- Uma rotina diária regrava os campos derivados para quem ainda lê o Firestore direto.

## Consequências

- O preço cobrado é sempre o correto.
- Enquanto o app não usar só a API, a regra existe também em Dart.
- Pedidos antigos, criados pelo app, precisam de tratamento especial (`origemApi` ausente).
