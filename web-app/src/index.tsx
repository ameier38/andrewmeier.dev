import React from 'react'
import ReactDOM from 'react-dom'
import { BrowserRouter as Router } from 'react-router-dom'
import { ClientContext } from 'graphql-hooks'
import { ThemeProvider } from '@material-ui/core/styles'
import * as serviceWorker from './serviceWorker'
import './index.css';
import { theme } from './theme'
import { client } from './graphql'
import { App } from './components/App'

ReactDOM.render(
    <React.StrictMode>
        <ClientContext.Provider value={client}>
            <Router>
                <ThemeProvider theme={theme}>
                    <App />
                </ThemeProvider>
            </Router>
        </ClientContext.Provider>
    </React.StrictMode>,
    document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
