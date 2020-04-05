import React from 'react'
import { makeStyles } from '@material-ui/core/styles'
import { 
    Drawer,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Divider
} from '@material-ui/core'
import { AccessTime } from '@material-ui/icons'
import { useAppState, DRAWER_TOGGLED } from '../state'

const useStyles = makeStyles({
    list: {
        width: 250
    }
})

export const AppDrawer = () => {
    const classes = useStyles()
    const [state, dispatch] = useAppState()
    const onClose = () => dispatch ({ type: DRAWER_TOGGLED, payload: { open: false } })
    return (
        <Drawer 
            variant='temporary'
            anchor='left'
            open={state.drawer.open}
            onClose={onClose} >
            <List className={classes.list}>
                <ListItem>
                    <ListItemIcon><AccessTime/></ListItemIcon>
                    <ListItemText>Archive</ListItemText>
                </ListItem>
                <Divider/>
                <ListItem>
                    <ListItemText>First Post</ListItemText>
                </ListItem>
            </List>
        </Drawer>
    )
}
