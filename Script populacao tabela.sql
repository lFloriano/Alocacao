--------CLIENTES--------
INSERT INTO CLientes(
	Criado,
	Nome,
	CPFCNPJ,
	Setor
)
VALUES(
	GETDATE(),
	'Ind�strias ACME Inc.',
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
	'�de Brecha',
	'66.865.731/0001-08',
	'Constru��o civil'
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
	'Aliment�cio'
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
	'Aliment�cio'
)

--------GESTORES-----------
INSERT INTO Gestores(
	nome,
	Departamento
)
Values
('Isaac Newton', 'Inova��o'),
('Fiodor Dostoievsky', 'RH'),
('Rene Descartes', 'Big data'),
('Isaac Newton', 'Inova��o')

------COLABORADORES-----------

insert into Colaboradores Values
(getdate(), 'Keith Richards', 'Programador', 'Inova��o', getdate()),
(getdate(), 'Chuck Berry', 'Analista de neg�cio', 'Marketing', getdate()),
(getdate(), 'John Paul Jones', 'Designer', 'Marketing', getdate()),
(getdate(), 'Seasick Steve', 'Assistente de comunica��o', 'Marketing', getdate()),
(getdate(), 'Ronaldo McRonald', 'Designer', 'Inova��o', getdate()),
(getdate(), 'Bruce Wayne', 'Programador', 'Inova��o', getdate()),
(getdate(), 'Charles Darwin', 'Gerente de Projeto', 'Inova��o', getdate()),
(getdate(), 'Martin Luther', 'Assistente cont�bil', 'Contabilidade', getdate()),
(getdate(), 'Genghis Kahn', 'Assistente de comunica��o', 'Marketing', getdate()),
(getdate(), 'Napole�o Bonaparte', 'Gerente de Projeto', 'Inova��o', getdate()),
(getdate(), 'Poucas Trancas', 'Assistente de comunica��o', 'Marketing', getdate())