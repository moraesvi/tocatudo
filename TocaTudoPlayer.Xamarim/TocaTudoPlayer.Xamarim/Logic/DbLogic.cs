using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public class DbLogic : IDbLogic
    {
        private readonly IDatabaseConn _dbConn;
        public DbLogic(IDatabaseConn dbConn)
        {
            _dbConn = dbConn;
        }
        //public async Task<ApiSearchMusicModel[]> GetAlbums()
        //{
        //    List<ApiSearchMusicModel> lstAlbumModel = new List<ApiSearchMusicModel>();

        //    foreach (TbAlbum album in await _dbConn.Albums)
        //    {
        //        lstAlbumModel.Add(new ApiSearchMusicModel() { Id = Convert.ToInt16(album.Id), VideoId = album.VideoId, NomeAlbum = album.AlbumName, TipoParse = new int[] { 3 } });
        //    }

        //    return lstAlbumModel.ToArray();
        //}
        //public async Task<ApiSearchMusicModel[]> GetMusics()
        //{
        //    List<ApiSearchMusicModel> lstAlbumModel = new List<ApiSearchMusicModel>();

        //    foreach (TbMusic music in await _dbConn.Musics)
        //    {
        //        lstAlbumModel.Add(new ApiSearchMusicModel() { Id = Convert.ToInt16(music.Id), VideoId = music.VideoId, NomeAlbum = music.MusicName });
        //    }

        //    return lstAlbumModel.ToArray();
        //}
        //public async Task<bool> ExistsAlbumAsync(string videoId)
        //{
        //    return await _dbConn.ExistsAlbumAsync(videoId);
        //}
        //public async Task<bool> ExistsMusicAsync(string videoId)
        //{
        //    return await _dbConn.ExistsMusicAsync(videoId);
        //}
        //public async Task<int> InsertOrUpdateAsync(string uAlbumlId, AlbumModel album, (bool, byte[]) musicData)
        //{
        //    if (album == null)
        //        throw new ArgumentNullException("Parâmetros não informados");

        //    string musicPath = await album.GetFileNameLocalPath();

        //    TbAlbum tbAlbum = new TbAlbum();
        //    tbAlbum.UAlbumlId = uAlbumlId;
        //    tbAlbum.VideoId = album.VideoId;
        //    tbAlbum.AlbumName = album.Album;
        //    tbAlbum.IsAlbumMusicCompressed = musicData.Item1;
        //    tbAlbum.AlbumMusicPath = musicPath;

        //    return await _dbConn.InsertOrUpdateAlbumAsync(tbAlbum, musicData.Item2);
        //}
        //public async Task<int> InsertOrUpdateAsync(string uMusicId, MusicModel music, (bool, byte[]) musicData)
        //{
        //    if (music == null)
        //        throw new ArgumentNullException("Parâmetros não informados");

        //    string musicPath = await music.GetFileNameLocalPath();

        //    TbMusic tbMusic = new TbMusic();
        //    tbMusic.UMusicId = uMusicId;
        //    tbMusic.VideoId = music.VideoId;
        //    tbMusic.MusicName = music.MusicName;
        //    tbMusic.IsMusicCompressed = musicData.Item1;
        //    tbMusic.MusicPath = musicPath;

        //    return await _dbConn.InsertOrUpdateMusicAsync(tbMusic, musicData.Item2);
        //}
        //public async Task<(AlbumModel, byte[])> GetAlbumByVideoIdAsync(string videoId)
        //{
        //    (TbAlbum, byte[]) tbAlbum = await _dbConn.GetAlbumByVideoIdAsync(videoId);

        //    if (tbAlbum.Item1 == null || tbAlbum.Item2 == null)
        //        return (null, null);

        //    AlbumModel album = new AlbumModel();
        //    album.UAlbumlId = tbAlbum.Item1.UAlbumlId;
        //    album.VideoId = tbAlbum.Item1.VideoId;
        //    album.Album = tbAlbum.Item1.AlbumName;

        //    if(tbAlbum.Item1.IsAlbumMusicCompressed)
        //        tbAlbum.Item2 = await HttpDownload.Descompress(tbAlbum.Item2);

        //    return (album, tbAlbum.Item2);
        //}
        //public async Task<(MusicModel, byte[])> GetMusicByVideoIdAsync(string videoId)
        //{
        //    (TbMusic, byte[]) tbMusic = await _dbConn.GetMusicByVideoIdAsync(videoId);

        //    if (tbMusic.Item1 == null || tbMusic.Item2 == null)
        //        return (null, null);

        //    MusicModel music = new MusicModel();
        //    music.UMusicId = tbMusic.Item1.UMusicId;
        //    music.VideoId = tbMusic.Item1.VideoId;
        //    music.MusicName = tbMusic.Item1.MusicName;

        //    if (tbMusic.Item1.IsMusicCompressed)
        //        tbMusic.Item2 = await HttpDownload.Descompress(tbMusic.Item2);

        //    return (music, tbMusic.Item2);
        //}
        //public async Task<bool> DeleteAlbum(string videoId) 
        //{
        //    (TbAlbum, byte[]) tbAlbum = await _dbConn.GetAlbumByVideoIdAsync(videoId);

        //    if (tbAlbum.Item1 == null || tbAlbum.Item2 == null)
        //        return false;

        //    await _dbConn.DeleteAlbumAsync(tbAlbum.Item1);

        //    return true;
        //}
        //public async Task<bool> DeleteMusic(string videoId)
        //{
        //    (TbMusic, byte[]) tbMusic = await _dbConn.GetMusicByVideoIdAsync(videoId);

        //    if (tbMusic.Item1 == null || tbMusic.Item2 == null)
        //        return false;

        //    await _dbConn.DeleteMusicAsync(tbMusic.Item1);

        //    return true;
        //}
    }
}
