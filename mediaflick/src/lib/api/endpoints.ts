import { fetchApi } from '@/lib/api/client'
import {
    EpisodeInfo,
    MediaInfo,
    MediaSearchResult,
    MediaStatus,
    MediaType,
    PagedResult,
    ScannedFile,
    ScannedFileStats,
    SeasonInfo,
    UpdateScannedFileRequest,
} from '@/lib/api/types'

// API Methods
export const mediaApi = {
    // Media Lookup
    searchMovies: (title: string) =>
        fetchApi<MediaSearchResult[]>(`/medialookup/movies/search?title=${encodeURIComponent(title)}`),

    searchTvShows: (title: string) =>
        fetchApi<MediaSearchResult[]>(`/medialookup/tvshows/search?title=${encodeURIComponent(title)}`),

    getMovie: (tmdbId: number) => fetchApi<MediaInfo>(`/medialookup/movies/${tmdbId}`),

    getTvShow: (tmdbId: number) => fetchApi<MediaInfo>(`/medialookup/tvshows/${tmdbId}`),

    getTvSeason: (tmdbId: number, seasonNumber: number) =>
        fetchApi<MediaInfo>(`/medialookup/tvshows/${tmdbId}/seasons/${seasonNumber}`),

    getTvEpisode: (tmdbId: number, seasonNumber: number, episodeNumber: number) =>
        fetchApi<EpisodeInfo>(`/medialookup/tvshows/${tmdbId}/seasons/${seasonNumber}/episodes/${episodeNumber}`),

    getImageUrl: (path: string, size: string = 'w500') => fetchApi<string>(`/medialookup/images/${path}?size=${size}`),

    // Scanned Files
    getScannedFiles: (params: { page?: number; pageSize?: number; sortBy?: string; sortOrder?: string }) => {
        const queryParams = new URLSearchParams({
            page: (params.page || 1).toString(),
            pageSize: (params.pageSize || 10).toString(),
            sortBy: (params.sortBy || 'createdAt').toString(),
            sortOrder: (params.sortOrder || 'desc').toString(),
        })
        return fetchApi<PagedResult<ScannedFile>>(`/scannedfiles?${queryParams.toString()}`)
    },

    getScannedFile: (id: number) => fetchApi<ScannedFile>(`/scannedfiles/${id}`),

    getScannedFileStats: () => fetchApi<ScannedFileStats>('/scannedfiles/stats'),

    updateScannedFile: (id: number, data: UpdateScannedFileRequest) =>
        fetchApi<ScannedFile>(`/scannedfiles/${id}`, {
            method: 'PATCH',
            body: JSON.stringify(data),
        }),

    recreateSymlink: (id: number) =>
        fetchApi<ScannedFile>(`/scannedfiles/${id}/recreate-symlink`, {
            method: 'PATCH',
        }),

    deleteScannedFile: (id: number) =>
        fetchApi(`/scannedfiles/${id}`, {
            method: 'DELETE',
        }),

    deleteScannedFiles: (ids: number[]) =>
        fetchApi('/scannedfiles', {
            method: 'DELETE',
            body: JSON.stringify(ids),
        }),

    recreateAllSymlinks: () =>
        fetchApi<{ successCount: number }>('/scannedfiles/recreate-symlinks', {
            method: 'POST',
        }),

    updateImdbIds: (batchSize: number = 50) =>
        fetchApi<{ updated: number; failed: number }>(`/scannedfiles/update-imdb-ids?batchSize=${batchSize}`, {
            method: 'POST',
        }),

    // Symlink Management
    cleanupDeadSymlinks: () =>
        fetchApi<{ message: string }>('/symlink/cleanup', {
            method: 'POST',
        }),
}