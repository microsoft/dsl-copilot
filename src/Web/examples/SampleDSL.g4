grammar SampleDSL;

// Lexer rules
ID : [a-zA-Z]+ ;
INT : [0-9]+ ;
WS : [ \t\r\n]+ -> skip ;

// Parser rules
program : statement+ ;

statement : assignment
          | printStatement
          ;

assignment : ID '=' expression ';' ;

printStatement : 'print' expression ';';

expression : ID
           | INT
           | expression '+' expression
           | expression '-' expression
           | '(' expression ')'
           ;