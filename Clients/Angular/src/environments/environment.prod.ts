let hostName = new URL(window.location.href).host;

export const environment = {
  production: true,
  baseURL: '/',
  apiURL: '/api/',
  wsURL: `ws://${hostName}/api/`,
  blocklyMedia: 'assets/'
};
