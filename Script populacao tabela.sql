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

-------PROJETOS---------------
insert into projetos(
	criado, nome, Descricao, Responsavel, DataInicio, DataFim, idcliente, statusProjeto)
values
	(getdate(), 'Projeto A','Exemplo de texto com a descri��o do projeto', 1, '2019-05-04', '2019-09-04', 1, 'Em andamento'),
	(getdate(), 'Projeto B','Exemplo de texto com a descri��o do projeto', 1, '2019-01-14', '2019-09-04', 4, 'Em andamento'),
	(getdate(), 'Projeto C','Exemplo de texto com a descri��o do projeto', 2, '2019-05-04', '2020-02-11', 5, 'Pausado'),
	(getdate(), 'Projeto D','Exemplo de texto com a descri��o do projeto', 2, '2020-11-04', '2022-09-22', 2, 'Em Planejamento'),
	(getdate(), 'Projeto E','Exemplo de texto com a descri��o do projeto', 3, '2019-05-04', '2019-09-04', 5, 'Pausado'),
	(getdate(), 'Projeto F','Exemplo de texto com a descri��o do projeto', 3, '2019-05-04', '2019-09-04', 4, 'Em andamento'),
	(getdate(), 'Projeto G','Exemplo de texto com a descri��o do projeto', 4, '2019-08-04', '2019-12-12', 1, 'Em andamento'),
	(getdate(), 'Projeto H','Exemplo de texto com a descri��o do projeto', 4, '2019-01-04', '2019-10-14', 3, 'Pausado'),
	(getdate(), 'Projeto I','Exemplo de texto com a descri��o do projeto', 5, '2019-01-01', '2019-07-29', 4, 'Em andamento'),
	(getdate(), 'Projeto J','Exemplo de texto com a descri��o do projeto', 5, '2019-02-02', '2019-10-22', 3, 'Em andamento')