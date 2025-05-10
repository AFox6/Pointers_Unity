GRAMMAR=RobotC.g4
all:
	antlr4 -Dlanguage=CSharp $(GRAMMAR)

test:
	antlr4-parse $(GRAMMAR) statements -tree
