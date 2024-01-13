using IcotakuScrapper.Objects;

namespace IcotakuScrapperTest;

public class WebserverDirectoryTest
{
    [Test]
    public async Task ScrapAsyncTest()
    {
        var directory = new WebServerDirectoryIndex(new Uri("https://anime.icotaku.com/uploads/chroniques/chronique_10/"));
        await directory.LoadAsync();
        await directory.LoadSubDirectoryAsync();
        await directory.LoadParentDirectoryAsync();
        
        if (directory.ParentDirectory != null)
            await directory.ParentDirectory.LoadSubDirectoryAsync();
        
        if (!directory.IsWebServerDirectoryUrl)
        {
            Assert.Fail("L'URL ne correspond pas à un répertoire de serveur web.");
            return;
        }

        Console.WriteLine(directory.SubDirectories.Length);
        Console.WriteLine(directory.ParentDirectory);
        if (directory.ParentDirectory != null)
            Console.WriteLine(directory.ParentDirectory.SubDirectories.Length);
        Console.WriteLine(directory.RootDistance);
        Console.WriteLine(directory.IsRootDirectory);
        Console.WriteLine(directory.IsWebServerDirectoryUrl);
        Console.WriteLine(directory.BaseUri);
        Console.WriteLine(directory.ParentUri);
        Console.WriteLine(directory.HasChildrens);
        Console.WriteLine(directory.HasDirectories);
        Console.WriteLine(directory.HasFiles);
        Console.WriteLine(directory.DirectoryContents.Length);
        
        Assert.Pass();
    }
}