# 03. Como rodar localmente

Este tutorial leva do clone até o painel aberto, com uma loja cadastrada. Leva cerca de 20 minutos na primeira vez.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download).
- Visual Studio 2022 (ou mais novo) com a carga "Desenvolvimento ASP.NET e Web", ou VS Code com a extensão C# Dev Kit.
- Acesso ao projeto Firebase `ivalid-9396b` (peça para alguém da equipe adicionar seu e-mail no console do Firebase).
- Git.

## 1. Clonar

```bash
git clone https://github.com/fadertechltda/IValid_Web.git
cd IValid_Web
dotnet restore IValid.slnx
```

## 2. Credencial do Firebase (conta de serviço)

A API acessa o Firestore e envia push com uma conta de serviço. O arquivo **não** vai para o Git.

1. No [console do Firebase](https://console.firebase.google.com/), abra o projeto e vá em **Configurações do projeto → Contas de serviço → Gerar nova chave privada**.
2. Salve o arquivo como `Chave/Chave_Firebase.json`, na raiz do repositório (ao lado de `IValid.slnx`).

O caminho é lido de `Firebase:CredentialsPath` (`../Chave/Chave_Firebase.json`), relativo à pasta `WEB_API`.

Confira também, em **Authentication → Método de login**, se o provedor **E-mail/senha** está ativado. Sem ele, o cadastro de lojas falha com `OPERATION_NOT_ALLOWED`.

## 3. Configuração local

Os dois projetos precisam da **mesma** chave interna. Os arquivos `appsettings.Development.json` já existem com um valor de desenvolvimento:

`WEB_API/appsettings.Development.json`
```json
{
  "Seguranca": {
    "ApiKeyInterna": "dev-ivalid-internal-8f3ac2b1"
  }
}
```

`WEB/appsettings.Development.json`
```json
{
  "Seguranca": {
    "ApiKeyInterna": "dev-ivalid-internal-8f3ac2b1"
  }
}
```

Opcional, mas recomendado:

- **E-mail (SMTP):** preencha a seção `Email` em `WEB_API/appsettings.json` (ou, melhor, em `appsettings.Development.json`) para receber o código de acesso da loja no cadastro. Com Gmail, use uma [senha de app](https://support.google.com/accounts/answer/185833). Se o e-mail falhar, o cadastro **continua** sem avisar, e o código precisa ser consultado no Firestore (`supermercados/{id}.codigoAcesso`) ou no portal da plataforma.
- **Portal da plataforma:** coloque seu e-mail em `Plataforma:EmailsAdministradores` e crie esse usuário no Firebase Authentication.
- **Rotinas:** para não alterar dados compartilhados enquanto desenvolve, desligue com `"Rotinas": { "Habilitada": false }`.

Todas as chaves estão em [Configuração](04-configuracao.md).

## 4. Rodar

São dois processos. A API precisa estar no ar antes de usar o painel.

**Visual Studio:** clique com o botão direito na solução → **Configurar projetos de inicialização** → **Vários projetos de inicialização** → marque `WEB_API` e `WEB` como "Iniciar", com o perfil `https`.

**Terminal:**

```bash
dotnet run --project WEB_API --launch-profile https
```

Em outro terminal:

```bash
dotnet run --project WEB --launch-profile https
```

| Serviço | URL |
|---|---|
| API | `https://localhost:7183` (HTTP em `http://localhost:5170`) |
| Documentação interativa da API (Scalar) | `https://localhost:7183/scalar` |
| Painel | `https://localhost:7167` |

O painel encontra a API por `ApiSettings:BaseUrl`, que por padrão é `https://localhost:7183/`.

Se o navegador reclamar do certificado, rode `dotnet dev-certs https --trust` uma vez.

## 5. Primeiro acesso

1. Abra o painel e clique em **Registre-se**.
2. Preencha seus dados e os da loja. O sistema cria o usuário no Firebase Authentication, o documento em `supermercados` e o administrador em `users`.
3. Pegue o **código de acesso** no e-mail (ou no Firestore).
4. Na tela de login, informe o código, escolha seu nome e digite a senha.

Para entrar como equipe IValid, use o link **Acesso da equipe IValid** na tela de login.

## 6. Testar as rotas do app

O arquivo `WEB_API/WEB_API.http` tem exemplos prontos para o Visual Studio ou para a extensão REST Client do VS Code. Para as rotas autenticadas, cole um `idToken` válido de um cliente (gerado pelo app ou pela API REST do Firebase Auth).

A tela do Scalar já mostra quais rotas pedem `X-Internal-Api-Key` e quais pedem `Bearer`.

## Desenvolver sem mexer nos dados reais

O Firestore é um só para todos. Para trabalhar isolado, use o emulador:

```bash
npm install -g firebase-tools
firebase emulators:start --only firestore
```

E suba a API com a variável `FIRESTORE_EMULATOR_HOST=127.0.0.1:8080`. O `FirestoreDbBuilder` usa `EmulatorDetection.EmulatorOrProduction`, então detecta a variável sozinho.

Limitação: a autenticação continua indo para o Firebase real, porque a API chama a API REST do Firebase Auth (`identitytoolkit.googleapis.com`) diretamente.

## Problemas comuns

| Sintoma | Causa provável |
|---|---|
| Painel mostra "Não foi possível conectar ao servidor" | API fora do ar ou `ApiSettings:BaseUrl` errado. |
| API responde `401 Acesso negado: esta API é de uso interno` | `Seguranca:ApiKeyInterna` diferente entre `WEB` e `WEB_API`. |
| API responde `500` com "chave interna ausente" | `Seguranca:ApiKeyInterna` não configurada na API. |
| Erro de credencial do Google ao listar produtos | `Chave/Chave_Firebase.json` ausente ou de outro projeto. |
| Cadastro falha com `OPERATION_NOT_ALLOWED` | Provedor E-mail/senha desativado no Firebase Authentication. |
| Login aceita, mas volta para a tela de login | O painel foi aberto por HTTP. O cookie é `Secure`, então use o perfil `https`. |
| Login da plataforma sempre negado | E-mail fora de `Plataforma:EmailsAdministradores` ou usuário inexistente no Firebase Auth. |
