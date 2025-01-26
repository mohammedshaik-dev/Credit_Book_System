-- Database: CreditBookDB

-- DROP DATABASE IF EXISTS "CreditBookDB";

CREATE DATABASE "CreditBookDB"
    WITH
    OWNER = postgres
    ENCODING = 'WIN1252'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    LOCALE_PROVIDER = 'libc'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- Table: public.creditentries

-- DROP TABLE IF EXISTS public.creditentries;

CREATE TABLE IF NOT EXISTS public.creditentries
(
    id integer NOT NULL DEFAULT nextval('creditentries_id_seq'::regclass),
    date date NOT NULL,
    itemname character varying(255) COLLATE pg_catalog."default" NOT NULL,
    quantity integer NOT NULL,
    rate numeric(10,2) NOT NULL,
    totalamount numeric(10,2) GENERATED ALWAYS AS (((quantity)::numeric * rate)) STORED,
    remainingbalance numeric(10,2) NOT NULL,
    CONSTRAINT creditentries_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.creditentries
    OWNER to postgres;
-- Table: public.settlements

-- DROP TABLE IF EXISTS public.settlements;

CREATE TABLE IF NOT EXISTS public.settlements
(
    id integer NOT NULL DEFAULT nextval('settlements_id_seq'::regclass),
    date date NOT NULL,
    amountpaid numeric(10,2) NOT NULL,
    remainingbalance numeric(10,2) NOT NULL,
    creditentryid integer NOT NULL,
    CONSTRAINT settlements_pkey PRIMARY KEY (id),
    CONSTRAINT fk_creditentry FOREIGN KEY (creditentryid)
        REFERENCES public.creditentries (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.settlements
    OWNER to postgres;