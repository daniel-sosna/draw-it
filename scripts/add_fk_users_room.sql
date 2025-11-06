ALTER TABLE public.users
ADD CONSTRAINT fk_users_room
FOREIGN KEY ("RoomId")
REFERENCES public.rooms ("Id")
ON DELETE SET NULL;

