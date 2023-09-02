import { create } from "zustand";
import { createWithEqualityFn } from "zustand/traditional";

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  searchTerm: string;
  searchValue: string;
  filterBy: string;
  orderBy: string;
};

type Actions = {
  setParams: (params: Partial<State>) => void;
  reset: () => void;
  setSearchValue: (value: string) => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 12,
  pageCount: 1,
  searchTerm: "",
  searchValue: "",
  filterBy: "live",
  orderBy: "make",
};

export const useParamsStore = createWithEqualityFn<State & Actions>()(
  (set) => ({
    ...initialState,

    setParams: (newParams: Partial<State>) => {
      set((state) => {
        if (newParams.pageNumber) {
          return { ...state, pageNumber: newParams.pageNumber };
        } else {
          return { ...state, ...newParams, pageNumber: 1 };
        }
      });
    },

    reset: () => set(initialState),

    setSearchValue: (value: string) => {
      set({ searchValue: value });
    },
  }),
  Object.is
);