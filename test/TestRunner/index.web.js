import React, {
  AppRegistry,
} from 'react-native';

import SQLiteDemo from './sqlitedemo'

AppRegistry.registerComponent('TestRunner', () => SQLiteDemo);

import {Platform} from 'react-native';

if(Platform.OS == 'winjs'){
  var app = document.createElement('div');
  document.body.appendChild(app);

  AppRegistry.runApplication('TestRunner', {
    rootTag: app
  })
}
