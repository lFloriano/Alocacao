--------CLIENTES--------
INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Indústrias ACME Inc.',
	'21.551.031/0001-00',
	'Diversos'
)

INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Alva & Alva Stockings',
	'45.244.353/0001-75',
	'Textil'
)

INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Óde Brecha',
	'66.865.731/0001-08',
	'Construção civil'
)

INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Jota BS',
	'07.514.028/0001-96',
	'Alimentício'
)

INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Free Boi',
	'64.185.368/0001-54',
	'Alimentício'
)

--------GESTORES-----------
INSERT INTO Gestores(
	nome,
	Departamento
)
Values
('Isaac Newton', 'Inovação'),
('Fiodor Dostoievsky', 'RH'),
('Rene Descartes', 'Big data'),
('Isaac Newton', 'Inovação')

------COLABORADORES-----------

insert into Colaboradores Values
(getdate(), 'Keith Richards', 'Programador', 'Inovação', getdate()),
(getdate(), 'Chuck Berry', 'Analista de negócio', 'Marketing', getdate()),
(getdate(), 'John Paul Jones', 'Designer', 'Marketing', getdate()),
(getdate(), 'Seasick Steve', 'Assistente de comunicação', 'Marketing', getdate()),
(getdate(), 'Ronaldo McRonald', 'Designer', 'Inovação', getdate()),
(getdate(), 'Bruce Wayne', 'Programador', 'Inovação', getdate()),
(getdate(), 'Charles Darwin', 'Gerente de Projeto', 'Inovação', getdate()),
(getdate(), 'Martin Luther', 'Assistente contábil', 'Contabilidade', getdate()),
(getdate(), 'Genghis Kahn', 'Assistente de comunicação', 'Marketing', getdate()),
(getdate(), 'Napoleão Bonaparte', 'Gerente de Projeto', 'Inovação', getdate()),
(getdate(), 'Poucas Trancas', 'Assistente de comunicação', 'Marketing', getdate())