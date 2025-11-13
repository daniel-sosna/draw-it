-- Enforce same rules as backend validation
-- DrawingTime: 20..300 seconds (nullable for legacy rows)
ALTER TABLE public.rooms
  ADD CONSTRAINT chk_rooms_drawing_time
    CHECK ("DrawingTime" IS NULL OR "DrawingTime" BETWEEN 20 AND 300);

-- NumberOfRounds: >= 1 (nullable for legacy rows)
ALTER TABLE public.rooms
  ADD CONSTRAINT chk_rooms_num_rounds
    CHECK ("NumberOfRounds" IS NULL OR "NumberOfRounds" >= 1);

