//using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class SQLiteConn : IDatabaseConn
    {
        //private string _dbPath;
        //private SQLiteAsyncConnection _sqlConn;
        //public SQLiteConn()
        //{
        //    _dbPath = App._dbLocalPath;
        //}
        //public bool DatabaseExists
        //{
        //    get
        //    {
        //        return File.Exists(_dbPath);
        //    }
        //}
        //public async Task CreateDatabaseIfNotExists()
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    await _sqlConn.CreateTableAsync<TbAlbum>();
        //    await _sqlConn.CreateTableAsync<TbMusic>();
        //}
        //public Task<TbAlbum[]> Albums
        //{
        //    get
        //    {
        //        _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //        return _sqlConn.Table<TbAlbum>()
        //                        .DateOrderByDescending()
        //                        .ToArrayAsync();
        //    }
        //}
        //public Task<TbMusic[]> Musics
        //{
        //    get
        //    {
        //        _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //        return _sqlConn.Table<TbMusic>()
        //                       .DateOrderByDescending()
        //                       .ToArrayAsync();
        //    }
        //}
        //public async Task<TbAlbum> GetAlbumByIdAsync(int idAlbum)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    TbAlbum result = await _sqlConn.Table<TbAlbum>()
        //                                   .Where(album => album.Id == idAlbum)
        //                                   .FirstOrDefaultAsync();

        //    return result;
        //}
        //public async Task<TbMusic> GetMusicByIdAsync(int musicId)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    TbMusic result = await _sqlConn.Table<TbMusic>()
        //                                   .Where(music => music.Id == musicId)
        //                                   .FirstOrDefaultAsync();

        //    return result;
        //}
        //public async Task<TbAlbum[]> GetAlbumsByNameAsync(string albumName)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    TbAlbum[] result = await _sqlConn.Table<TbAlbum>()
        //                                     .Where(album => album.AlbumName == albumName)
        //                                     .DateOrderByDescending()
        //                                     .ToArrayAsync();

        //    return result;
        //}
        //public async Task<(TbAlbum, byte[])> GetAlbumByVideoIdAsync(string videoId)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    TbAlbum tbAlbum = await _sqlConn.Table<TbAlbum>().Where(album => album.VideoId == videoId).FirstOrDefaultAsync();
        //    byte[] musicData = await File.ReadAllBytesAsync(tbAlbum.AlbumMusicPath);

        //    return (tbAlbum, musicData);
        //}
        //public async Task<(TbMusic, byte[])> GetMusicByVideoIdAsync(string videoId)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    TbMusic tbMusic = await _sqlConn.Table<TbMusic>().Where(album => album.VideoId == videoId).FirstOrDefaultAsync();
        //    byte[] musicData = await File.ReadAllBytesAsync(tbMusic.MusicPath);

        //    return (tbMusic, musicData);
        //}
        //public async Task<bool> ExistsAlbumAsync(string videoId)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    bool result = await _sqlConn.Table<TbAlbum>()
        //                                .Where(album => album.VideoId == videoId)
        //                                .FirstOrDefaultAsync() != null;

        //    return result;
        //}
        //public async Task<bool> ExistsMusicAsync(string videoId)
        //{
        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    bool result = await _sqlConn.Table<TbMusic>()
        //                                .Where(music => music.VideoId == videoId)
        //                                .FirstOrDefaultAsync() != null;

        //    return result;
        //}
        //public async Task<int> InsertOrUpdateAlbumAsync(TbAlbum album, byte[] musicData)
        //{
        //    if (album == null)
        //        return -1;

        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    DateTimeOffset dtTran = DateTimeOffset.UtcNow;
        //    int idAlbum = 0;

        //    if (album.Id == 0)
        //    {
        //        album.DtAdd = dtTran;

        //        idAlbum = await _sqlConn.InsertAsync(album);
        //        await File.WriteAllBytesAsync(album.AlbumMusicPath, musicData);
        //    }
        //    else
        //    {
        //        album.DtUpd = dtTran;
        //        idAlbum = await _sqlConn.UpdateAsync(album);
        //    }

        //    return idAlbum;
        //}
        //public async Task<int> InsertOrUpdateMusicAsync(TbMusic music, byte[] musicData)
        //{
        //    if (music == null)
        //        return -1;

        //    _sqlConn = new SQLiteAsyncConnection(_dbPath);

        //    DateTimeOffset dtTran = DateTimeOffset.UtcNow;
        //    int idAlbum = 0;

        //    if (music.Id == 0)
        //    {
        //        music.DtAdd = dtTran;

        //        idAlbum = await _sqlConn.InsertAsync(music);
        //        await File.WriteAllBytesAsync(music.MusicPath, musicData);
        //    }
        //    else
        //    {
        //        music.DtUpd = dtTran;
        //        idAlbum = await _sqlConn.UpdateAsync(music);
        //    }

        //    return idAlbum;
        //}
        //public async Task DeleteAlbumAsync(TbAlbum album)
        //{
        //    if (album == null)
        //        return;

        //    await _sqlConn.DeleteAsync(album);           
        //}
        //public async Task DeleteMusicAsync(TbMusic music)
        //{
        //    if (music == null)
        //        return;

        //    await _sqlConn.DeleteAsync(music);
        //}
    }
    internal static class DbExtensionG
    {
        //public static AsyncTableQuery<TbAlbum> DateOrderByDescending(this AsyncTableQuery<TbAlbum> albums) => albums.OrderByDescending(src => src.DtAdd)
        //                                                                                                            .ThenByDescending(src => src.DtUpd);
        //public static AsyncTableQuery<TbMusic> DateOrderByDescending(this AsyncTableQuery<TbMusic> albums) => albums.OrderByDescending(src => src.DtAdd)
        //                                                                                                            .ThenByDescending(src => src.DtUpd);
    }
    internal class AlbumQueries
    {
        public static string QueryAlbumByVideoId()
        {
            return $@"select ta.id
                             ,ta.video_id
                             ,ta.album_name
                             ,ta.author
                             ,tmd.music
                             ,tmd.music_image   
                             from tb_album ta
                             join tb_music_data tmd on tmd.fk_album = ta.id
                             where ta.video_id = ?";
        }
    }
}
