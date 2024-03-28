# Summary
Domain-Specific Languages (DSLs) are specialized programming languages designed for specific problem domains or application contexts. Companies use DSLs because they offer precision and productivity. Unlike general-purpose languages (such as Python, Java, or C++), DSLs are tailored to address particular tasks within a specific industry, field, or domain. DSLs also give companies the ability to customize software solutions to their specific needs. By creating DSLs tailored to their business processes, companies can adapt existing tools or frameworks without extensive modifications. Salesforce’s Apex language, designed for customizing their CRM platform, is an example of such adaptability

OpenAI has trained models on a vast amounts of existing text data, including coding languages and documentation. By leveraging this extensive training, GPT-based models can generate contextually relevant responses for code generation and code completion. Companies and developers utilize these models to enhance software development, automate code writing, and improve programming-related tasks.

OpenAI has the capability to address code based questions related to languages that the model has been exposed to during its training. However, since most Domain-Specific Languages (DSLs) are proprietary and not publicly available, Large Language Models (LLMs) typically require additional context to effectively answer questions or generate code in these languages. The following is a series of vetted steps that have been proven to enhance an LLM’s proficiency in interpreting a custom DSL language.

## Architecture

# Grammar File
Code grammar files serve as essential components used by code editors to tokenize and highlight source code. These files break down code into smaller units known as tokens and associate them with specific scopes for syntax highlighting. Editors like Visual Studio Code rely on TextMate grammars, which define rules for tokenization using regular expressions. Additionally, grammar files include a repository of language elements (such as functions and comments) and patterns that correspond to these elements. By visually distinguishing keywords, strings, and other code components, code grammar files significantly enhance readability. Importantly, each programming language has its own dedicated grammar file, defining language-specific rules and features.

During our testing, we discovered that using a grammar file for Domain-Specific Languages (DSLs) in the system prompt enabled us to anchor the Language Model (LLM) within the specific language context. This allowed the LLM to respond effectively to questions related to code writing. The grammar file provides a structural outline of the language, and although the LLM’s responses were formatted in DSL syntax, the LLM still hallucinated responses based on assumptions about the language layout.

## ANTLR
For our testing we utilized ANTLR (ANother Tool for Language Recognition) which is a parser generator used for processing structured text. ANTLR provides language processing primitives such as lexers, grammars, and parsers, along with the runtime to process text using them. ANTLR helps create parsers that transform a piece of text into an organized structure called a parse tree or Abstract Syntax Tree (AST). The AST represents the logical content of code, assembled by combining various elements. To obtain a parse tree, you define a lexer and parser grammar, invoke ANTLR to generate the lexer and parser in your target language (e.g., Java, Python, C#), and then use these generated components to recognize and construct the parse tree. ANTLR enables you to work with various languages, data formats, and other text-based structures, making it a valuable tool for language processing and development

# Code Validation/Compilation

# Linting

# Feedback loop and documented examples
