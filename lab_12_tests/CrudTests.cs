using lab12;

namespace lab_12_tests;

public class CrudTests : IAsyncLifetime
{
    // чистим БД перед тестами, чтоб не мешали друг другу
    public async Task InitializeAsync()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        db.Notes.RemoveRange(db.Notes);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask; // после теста ничего не делаем

    // тесты заметок
    [Fact]
    public async Task Create_Note()
    {
        var user = await Crud.CreateUser("тестовый юзер", CancellationToken.None);
        var note = await Crud.Create("проверка создания", DateTimeOffset.Now, user.Id, CancellationToken.None);

        Assert.True(note.Id > 0);
        Assert.Equal("проверка создания", note.Text);
        Assert.Equal(user.Id, note.UserId);
    }

    [Fact]
    public async Task Read_Search()
    {
        var user = await Crud.CreateUser("юзер для поиска", CancellationToken.None);
        var now = DateTimeOffset.Now;
        await Crud.Create("задача номер раз", now, user.Id, CancellationToken.None);
        await Crud.Create("важная задача", now, user.Id, CancellationToken.None);
        await Crud.Create("обычная запись", now, user.Id, CancellationToken.None);

        var results = await Crud.Read("задач");
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Read_ById()
    {
        var user = await Crud.CreateUser("поиск айди", CancellationToken.None);
        var target = await Crud.Create("найди меня", DateTimeOffset.Now, user.Id, CancellationToken.None);

        var found = await Crud.Read(target.Id);
        Assert.NotNull(found);
        Assert.Equal("найди меня", found.Text);
    }

    [Fact]
    public async Task Update_Note()
    {
        var user = await Crud.CreateUser("редактор", CancellationToken.None);
        var note = await Crud.Create("старый текст", DateTimeOffset.Now, user.Id, CancellationToken.None);

        await Crud.Update(note, "новый текст", DateTimeOffset.Now, CancellationToken.None);
        var updated = await Crud.Read(note.Id);

        Assert.Equal("новый текст", updated.Text);
    }

    [Fact]
    public async Task Delete_Note()
    {
        var user = await Crud.CreateUser("удаление", CancellationToken.None);
        var note = await Crud.Create("удали это", DateTimeOffset.Now, user.Id, CancellationToken.None);

        await Crud.Delete(note, CancellationToken.None);
        var deleted = await Crud.Read(note.Id);

        Assert.Null(deleted);
    }

    // тесты юзеров
    [Fact]
    public async Task Create_User()
    {
        var user = await Crud.CreateUser("новый чел", CancellationToken.None);

        Assert.True(user.Id > 0);
        Assert.Equal("новый чел", user.Name);
    }

    [Fact]
    public async Task ReadUser_ById()
    {
        var target = await Crud.CreateUser("ищем юзера", CancellationToken.None);
        var found = await Crud.ReadUser(target.Id);

        Assert.NotNull(found);
        Assert.Equal("ищем юзера", found.Name);
    }

    [Fact]
    public async Task ReadUsers_Search()
    {
        await Crud.CreateUser("Александр", CancellationToken.None);
        await Crud.CreateUser("Алексей", CancellationToken.None);
        await Crud.CreateUser("Мария", CancellationToken.None);

        var results = await Crud.ReadUsers("Алекс");
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateUser()
    {
        var user = await Crud.CreateUser("старое имя", CancellationToken.None);

        await Crud.UpdateUser(user, "обновленное имя", CancellationToken.None);
        var updated = await Crud.ReadUser(user.Id);

        Assert.Equal("обновленное имя", updated.Name);
    }

    [Fact]
    public async Task Delete_User()
    {
        var user = await Crud.CreateUser("под снос", CancellationToken.None);

        await Crud.DeleteUser(user, CancellationToken.None);
        var deleted = await Crud.ReadUser(user.Id);

        Assert.Null(deleted);
    }

    // проверяяем связь один юзер много заметышей (заметочек??) (замятёнышей?)
    [Fact]
    public async Task GetNotes_ByUser()
    {
        var owner = await Crud.CreateUser("владелец", CancellationToken.None);
        var other = await Crud.CreateUser("чужой", CancellationToken.None);
        var now = DateTimeOffset.Now;

        await Crud.Create("заметка 1", now, owner.Id, CancellationToken.None);
        await Crud.Create("заметка 2", now, owner.Id, CancellationToken.None);
        await Crud.Create("не моя", now, other.Id, CancellationToken.None);

        var notes = await Crud.GetNotesByUser(owner.Id);

        Assert.Equal(2, notes.Count);
    }
}