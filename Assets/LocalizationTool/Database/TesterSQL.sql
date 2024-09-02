Insert into categories (displayOrder, category, isDefault) values (2, "MainMenu", 0);
Insert into categories (displayOrder, category, isDefault) values (3, "Options", 0);
Insert into categories (displayOrder, category, isDefault) values (4, "Scene1", 0);

Insert into languages (displayOrder, language, isDefault) values (2, "Spanish", 0);

INSERT into translations_keys (key_name, categoryID, displayOrder) values ("mainMenu_title", 1, 1);
INSERT into translations_keys (key_name, categoryID, displayOrder) values ("options_title", 1, 1);

select tk.key_name, c.category from translations_keys tk, categories c where tk.categoryID = c.id;
select tk.key_name, c.category, l.language, t.translationText from translations_keys tk join categories c on tk.categoryID = c.id join translations t on tk.key_name = t.keyID join languages l on t.languageID = l.id;

INSERT into translations (keyID, languageID, translationText) values (1, 1, "HOLA");
INSERT into translations (keyID, languageID, translationText) values (1, 2, "ADIOS");
INSERT into translations (keyID, languageID, translationText) values (2, 1, "HOLA");
INSERT into translations (keyID, languageID, translationText) values (2, 2, "ADIOS");

select l.language, t.translationText from translations t join languages l on t.languageID = l.id where t.keyID = 'options_title';
select l.language, t.translationText from translations t join languages l on t.languageID = l.id ;
delete from translations where languageID = 2

UPDATE translations SET keyID = "title" WHERE keyID = "options_title";

select tk.key_name, c.category, t.translationText from translations_keys tk join categories c on tk.categoryID = c.id join translations t on tk.id = t.keyID
select id from languages


