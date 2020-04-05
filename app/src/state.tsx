import React, { createContext, useContext, useReducer, Dispatch } from 'react'

type AppState = {
    drawer: {
        open: boolean
    }
}

export const DRAWER_TOGGLED = 'DRAWER_TOGGLED'

export type DrawerToggled = {
    type: typeof DRAWER_TOGGLED,
    payload: {
        open: boolean
    }
}

type AppAction = DrawerToggled

const appReducer = (state:AppState, action:AppAction): AppState => {
    switch (action.type) {
        case DRAWER_TOGGLED:
            return {
                ...state,
                drawer: {
                    ...state.drawer,
                    open: action.payload.open
                }
            }
        default:
            return state
    }
} 

const initialAppState: AppState = {
    drawer: {
        open: false
    }
}

const initialAppDispatch: Dispatch<AppAction> = () => null

const AppContext = createContext<[AppState, Dispatch<AppAction>]>([initialAppState, initialAppDispatch])

export const AppStateProvider: React.FC = ({children}) => {
    const [state, dispatch] = useReducer(appReducer, initialAppState)
    return (
        <AppContext.Provider value={[state, dispatch]}>
            {children}
        </AppContext.Provider>
    )
}

export const useAppState = () => useContext(AppContext)
