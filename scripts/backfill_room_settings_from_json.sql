-- Copy values from JSONB column "Settings" into flat columns on rooms
-- Safe to run multiple times
UPDATE public.rooms
SET
  "RoomName"       = COALESCE("RoomName", "Settings"->>'roomName'),
  "CategoryId"     = COALESCE("CategoryId", NULLIF("Settings"->>'categoryId','')::bigint),
  "DrawingTime"    = COALESCE("DrawingTime", NULLIF("Settings"->>'drawingTime','')::int),
  "NumberOfRounds" = COALESCE("NumberOfRounds", NULLIF("Settings"->>'numberOfRounds','')::int)
WHERE "Settings" IS NOT NULL;