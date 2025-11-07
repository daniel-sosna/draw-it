ALTER TABLE public.rooms
  ADD COLUMN IF NOT EXISTS "RoomName" text,
  ADD COLUMN IF NOT EXISTS "CategoryId" bigint,
  ADD COLUMN IF NOT EXISTS "DrawingTime" int,
  ADD COLUMN IF NOT EXISTS "NumberOfRounds" int;

