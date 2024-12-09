import { ModeToggle } from "./theme-switcher";
import HeadButton from "@/components/HeadButton";
import { Home, Library, Popcorn, User } from "lucide-react";
import { Logo } from "@/components/Logo";

export default function HeadBar() {
  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="flex h-14 items-center px-4">
        <div className="flex items-center gap-8">
          <div className="flex gap-2 text-3xl font-bold"><span className="text-foreground">Media</span> <span className="text-primary uppercase tracking-wider">Flick</span></div>
          <div className="flex gap-2">
            <HeadButton icon={Home} href="/" label="Home" />
            <HeadButton icon={Library} href="/medialibrary" label="Media Library" />
            <HeadButton icon={Popcorn} href="/mediainfo" label="Media Info" />
            <HeadButton icon={User} href="/profile" label="Profile" />
          </div>
        </div>
        <div className="flex-1 flex justify-end">
          <ModeToggle />
        </div>
      </div>
    </header>
  );
}
