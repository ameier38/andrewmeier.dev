import React from 'react'
import ReactDOM from 'react-dom'
import { ClientContext } from 'graphql-hooks'
import './index.css';
import App from './components/App'
import { client } from './graphql'
import { AppStateProvider } from './state'
import * as serviceWorker from './serviceWorker'

ReactDOM.render(
    <React.StrictMode>
        <AppStateProvider>
            <ClientContext.Provider value={client}>
                <App />
            </ClientContext.Provider>
        </AppStateProvider>
    </React.StrictMode>,
    document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
