SET R10 0
SET R28 48
SET R27 4C
SET R26 57
SET R18 0

loopRead:
	SET R20 1
	JMP loopWait

loopWait: 
	EQ R19 R20 R18
	JMPT R19 loopCheck
	JMP loopWait

loopCheck:	
	EQ R0 R31 R30
	JMPT R0 end
	GT R21 R31 R30
	JMPT R21 loopGreater
	LT R22 R31 R30
	JMPT R22 loopLess

loopGreater:
	COP R25 R27
	SET R29 1
	JMP loopRead

loopLess: 
	COP R25 R28
	SET R29 1
	JMP loopRead

end: 
	COP R25 R26
	SET R29 1