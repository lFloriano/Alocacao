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

-------PROJETOS---------------
insert into projetos(
	criado, nome, Descricao, Responsavel, DataInicio, DataFim, idcliente, statusProjeto)
values
	(getdate(), 'Projeto A','Exemplo de texto com a descrição do projeto', 1, '2019-05-04', '2019-09-04', 1, 'Em andamento'),
	(getdate(), 'Projeto B','Exemplo de texto com a descrição do projeto', 1, '2019-01-14', '2019-09-04', 4, 'Em andamento'),
	(getdate(), 'Projeto C','Exemplo de texto com a descrição do projeto', 2, '2019-05-04', '2020-02-11', 5, 'Pausado'),
	(getdate(), 'Projeto D','Exemplo de texto com a descrição do projeto', 2, '2020-11-04', '2022-09-22', 2, 'Em Planejamento'),
	(getdate(), 'Projeto E','Exemplo de texto com a descrição do projeto', 3, '2019-05-04', '2019-09-04', 5, 'Pausado'),
	(getdate(), 'Projeto F','Exemplo de texto com a descrição do projeto', 3, '2019-05-04', '2019-09-04', 4, 'Em andamento'),
	(getdate(), 'Projeto G','Exemplo de texto com a descrição do projeto', 4, '2019-08-04', '2019-12-12', 1, 'Em andamento'),
	(getdate(), 'Projeto H','Exemplo de texto com a descrição do projeto', 4, '2019-01-04', '2019-10-14', 3, 'Pausado'),
	(getdate(), 'Projeto I','Exemplo de texto com a descrição do projeto', 5, '2019-01-01', '2019-07-29', 4, 'Em andamento'),
	(getdate(), 'Projeto J','Exemplo de texto com a descrição do projeto', 5, '2019-02-02', '2019-10-22', 3, 'Em andamento')