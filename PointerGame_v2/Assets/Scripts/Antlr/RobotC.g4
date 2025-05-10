grammar RobotC;

// statements : (statement)+ EOF ;

statement : (
        assignmentStmt
        // | declarationStmt
    ) ';' ;

// valueLiteral : GEM;
variableExpr : (modifier = ('*' | '&'))? VARNAME ;
// valueExpr:
	// valueLiteral
    // | 
    // variableExpr ;

assignmentStmt : (deref = '*')? VARNAME '=' variableExpr ;

// declarationStmt : (indirect = 'indirect')? (const = 'const') 'node' (pointer = '*')? VARNAME ('=' valueExpr)? ;

// GEM: 'GEM';
fragment ALPHA : [a-zA-Z] ;
fragment DIGIT : [0-9] ;
VARNAME: ALPHA (ALPHA | DIGIT | '_')*;
WHITESPACE: [ \t\r\n]-> skip;
COMMENT: '//' ~[\r\n]* -> skip;
