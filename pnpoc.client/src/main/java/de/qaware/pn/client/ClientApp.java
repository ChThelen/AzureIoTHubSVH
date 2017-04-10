package de.qaware.pn.client;

import com.microsoft.azure.sdk.iot.device.*;

import java.io.IOException;
import java.net.URISyntaxException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class ClientApp {
    private static String connString = "";
    private static IotHubClientProtocol protocol = IotHubClientProtocol.MQTT;
    private static DeviceClient client;

    private static class AppMessageCallback implements MessageCallback {
        public IotHubMessageResult execute(Message msg, Object context) {
            System.out.println("Received message from hub: "
                    + new String(msg.getBytes(), Message.DEFAULT_IOTHUB_MESSAGE_CHARSET));

            return IotHubMessageResult.COMPLETE;
        }
    }

    public static void main(String[] args) throws IOException, URISyntaxException {
        client = new DeviceClient(connString, protocol);

        MessageCallback callback = new AppMessageCallback();
        client.setMessageCallback(callback, null);
        client.open();

        System.out.println("Press ENTER to exit.");
        System.in.read();
        client.close();
    }
}
