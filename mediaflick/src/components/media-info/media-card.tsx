import Image from "next/image"

import { Card, CardBody, CardHeader, Chip, Spinner } from "@nextui-org/react"

import type { MediaInfo, MediaType } from "@/lib/api/types"
import { MediaType as MediaTypeEnum } from "@/lib/api/types"

export interface UniqueMediaEntry {
  tmdbId: number
  title: string
  mediaInfo?: MediaInfo
  isLoading: boolean
}

export interface MediaCardProps {
  media: UniqueMediaEntry
  mediaType: MediaType
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case "ended":
      return "default"
    case "returning series":
      return "success"
    case "in production":
      return "warning"
    case "planned":
      return "primary"
    case "canceled":
      return "danger"
    case "pilot":
      return "warning"
    default:
      return "default"
  }
}

const getImageUrlSync = (path: string) => {
  if (!path) return "/placeholder-image.jpg"
  return `https://image.tmdb.org/t/p/w500${path}`
}

export function MediaCard({ media, mediaType }: MediaCardProps) {
  return (
    <Card
      key={media.tmdbId}
      className="group relative h-[400px] overflow-hidden border-[1px] border-transparent ring-1 ring-white/10 transition-transform duration-200 [background:linear-gradient(theme(colors.background),theme(colors.background))_padding-box,linear-gradient(to_bottom_right,rgba(255,255,255,0.2),transparent_50%)_border-box] [box-shadow:inset_-1px_-1px_1px_rgba(0,0,0,0.1),inset_1px_1px_1px_rgba(255,255,255,0.1)] before:absolute before:inset-0 before:z-10 before:bg-gradient-to-br before:from-black/10 before:via-transparent before:to-black/30 after:absolute after:inset-0 after:bg-gradient-to-tr after:from-white/5 after:via-transparent after:to-white/10 hover:scale-[1.02] hover:shadow-xl"
      isBlurred
    >
      {media.mediaInfo?.posterPath && (
        <div className="absolute inset-0">
          <Image
            src={getImageUrlSync(media.mediaInfo.posterPath)}
            alt={media.title}
            fill
            className="object-cover transition-all duration-200 group-hover:scale-105 group-hover:brightness-[0.80]"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
            priority
          />
        </div>
      )}
      <CardHeader className="absolute z-20 flex-col items-start">
        {mediaType === MediaTypeEnum.TvShows && media.mediaInfo?.status && (
          <Chip size="sm" color={getStatusColor(media.mediaInfo.status)} variant="shadow" className="mb-2 shadow-lg">
            {media.mediaInfo.status}
          </Chip>
        )}
        <h4 className="text-xl font-medium text-white [text-shadow:0_2px_4px_rgba(0,0,0,0.8)] hover:text-white">
          {media.mediaInfo?.title || media.title}
        </h4>
        <p className="text-tiny text-white/80 [text-shadow:0_1px_2px_rgba(0,0,0,0.8)]">
          {media.mediaInfo?.year && `(${media.mediaInfo.year})`}
        </p>
      </CardHeader>
      <CardBody className="[&::-webkit-scrollbar]:auto absolute bottom-0 z-20 max-h-[200px] overflow-y-auto border-t-1 border-default-600/50 bg-black/50 bg-gradient-to-t from-black/50 via-black/30 to-transparent backdrop-blur-sm dark:border-default-100/50">
        {media.isLoading ? (
          <div className="flex justify-center">
            <Spinner size="sm" />
          </div>
        ) : media.mediaInfo ? (
          <div className="flex flex-col gap-2">
            {media.mediaInfo.genres && media.mediaInfo.genres.length > 0 && (
              <p className="text-tiny text-white/90 [text-shadow:0_1px_2px_rgba(0,0,0,0.8)]">
                {media.mediaInfo.genres.join(", ")}
              </p>
            )}
            {media.mediaInfo.summary && (
              <p className="line-clamp-3 text-tiny text-white/90 [text-shadow:0_1px_2px_rgba(0,0,0,0.8)]">
                {media.mediaInfo.summary}
              </p>
            )}
            {mediaType === MediaTypeEnum.TvShows && media.mediaInfo?.episodeCount && (
              <div className="mt-2">
                <div className="relative h-4 w-full overflow-hidden rounded-full bg-default-200/20">
                  <div
                    className={`absolute h-full ${
                      media.mediaInfo.episodeCountScanned === media.mediaInfo.episodeCount
                        ? "bg-primary-500"
                        : "bg-danger-500"
                    }`}
                    style={{
                      width: `${((media.mediaInfo.episodeCountScanned || 0) / media.mediaInfo.episodeCount) * 100}%`,
                    }}
                  />
                  <div className="absolute inset-0 flex items-center justify-center text-[10px] font-bold text-white [text-shadow:0_1px_2px_rgba(0,0,0,0.8)]">
                    {media.mediaInfo.episodeCountScanned || 0} / {media.mediaInfo.episodeCount}
                  </div>
                </div>
              </div>
            )}
          </div>
        ) : (
          <p className="text-tiny text-white/80 [text-shadow:0_1px_2px_rgba(0,0,0,0.8)]">
            No additional information available
          </p>
        )}
      </CardBody>
    </Card>
  )
}
